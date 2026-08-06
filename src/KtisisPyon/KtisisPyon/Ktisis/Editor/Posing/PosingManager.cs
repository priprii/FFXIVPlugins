using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Actions.Types;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Data.Files;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Attachment;
using Ktisis.Editor.Posing.AutoSave;
using Ktisis.Editor.Posing.Data;
using Ktisis.Editor.Posing.Ik;
using Ktisis.Editor.Posing.Types;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;

namespace Ktisis.Editor.Posing;

public class PosingManager : IPosingManager, IDisposable
{
	private record PoseState
	{
		public required PoseContainer Pose;

		[CompilerGenerated]
		[SetsRequiredMembers]
		protected PoseState(PoseState original)
		{
			Pose = original.Pose;
		}
	}

	private readonly IEditorContext _context;

	private readonly HookScope _scope;

	private readonly IFramework _framework;

	private readonly PoseAutoSave AutoSave;

	private readonly Dictionary<ushort, PoseState> _savedPoses = new Dictionary<ushort, PoseState>();

	public bool IsValid => _context.IsValid;

	public PoseMemento? StashedPose { get; set; }

	public DateTime? StashedAt { get; set; }

	public string? StashedFrom { get; set; }

	public IAttachManager Attachments { get; }

	public bool IsSolvingIk { get; set; }

	public bool IsIkEnabled { get; set; }

	private PosingModule? PoseModule { get; set; }

	private IkModule? IkModule { get; set; }

	public bool IsEnabled => PoseModule?.IsEnabled ?? false;

	public PosingManager(IEditorContext context, HookScope scope, IFramework framework, IAttachManager attach, PoseAutoSave autoSave)
	{
		_context = context;
		_scope = scope;
		_framework = framework;
		Attachments = attach;
		AutoSave = autoSave;
	}

	public void Initialize()
	{
		try
		{
			PoseModule = _scope.Create<PosingModule>(new object[1] { this });
			PoseModule.Initialize();
			IkModule = _scope.Create<IkModule>(new object[1] { this });
			IkModule.Initialize();
			AutoSave.Initialize(_context.Config);
			Subscribe();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to initialize posing manager:\n{value}");
		}
	}

	private unsafe void Subscribe()
	{
		PoseModule.OnSkeletonInit += OnSkeletonInit;
		PoseModule.OnDisconnect += OnDisconnect;
		_context.Characters.OnDisableDraw += OnDisableDraw;
		_context.Plugin.Config.OnSaved += AutoSave.Configure;
	}

	private unsafe void OnSkeletonInit(IGameObject gameObject, Skeleton* skeleton, ushort partialId)
	{
		RestorePoseFor(gameObject.ObjectIndex, skeleton, partialId);
	}

	private void OnDisconnect()
	{
		if (_context.Config.AutoSave.Enabled && _context.Config.AutoSave.OnDisconnect)
		{
			Ktisis.Log.Verbose("Disconnected, triggering pose save.");
			AutoSave.Save();
		}
	}

	private unsafe void OnDisableDraw(IGameObject gameObject, DrawObject* drawObject)
	{
		Ktisis.Log.Verbose($"Preserving state for {gameObject.Name} ({gameObject.ObjectIndex})");
		Skeleton* skeleton = gameObject.GetSkeleton();
		if (skeleton != null)
		{
			Attachments.Invalidate(skeleton);
			PreservePoseFor(gameObject.ObjectIndex, skeleton);
		}
	}

	public void SetEnabled(bool enable)
	{
		if (enable && !IsValid)
		{
			return;
		}
		if (!enable && _context.Config.AutoSave.Enabled && _context.Config.AutoSave.OnDisable)
		{
			Ktisis.Log.Verbose("Posing disabled, triggering pose save.");
			try
			{
				AutoSave.Save();
			}
			catch (Exception ex)
			{
				Ktisis.Log.Error(ex.ToString());
			}
		}
		HavokPosing.ClearCachedAbdomenModelTransform();
		PoseModule?.SetEnabled(enable);
	}

	public Task SyncFaceModelSpace(ActorEntity actor)
	{
		return _framework.RunOnTick((Func<Task>)async delegate
		{
			EntityPose pose = actor.Pose;
			if (pose != null)
			{
				EntityPoseConverter converter = new EntityPoseConverter(pose);
				PoseContainer initial = converter.Save();
				await _framework.RunOnTick((Action)delegate
				{
					PoseModule?.SyncFaceModelSpace(actor);
				}, default(TimeSpan), 0, default(CancellationToken));
				await _framework.RunOnTick((Action)delegate
				{
					PoseModule?.SyncFaceModelSpace(actor);
				}, default(TimeSpan), 0, default(CancellationToken));
				PoseContainer final = converter.Save();
				_context.Actions.History.Add(new PoseMemento(converter)
				{
					Modes = PoseMode.All,
					Transforms = (PoseTransforms.Rotation | PoseTransforms.Position),
					Bones = null,
					Initial = initial,
					Final = final
				});
			}
		}, default(TimeSpan), 0, default(CancellationToken));
	}

	public IIkController CreateIkController()
	{
		return IkModule.CreateController();
	}

	private unsafe void PreservePoseFor(ushort objectIndex, Skeleton* skeleton)
	{
		PoseContainer poseContainer = new PoseContainer();
		poseContainer.Store(skeleton);
		_savedPoses[objectIndex] = new PoseState
		{
			Pose = poseContainer
		};
	}

	private unsafe void RestorePoseFor(ushort objectIndex, Skeleton* skeleton, ushort partialId)
	{
		if (_savedPoses.TryGetValue(objectIndex, out PoseState value))
		{
			value.Pose.ApplyToPartial(skeleton, partialId, PoseTransforms.Rotation | PoseTransforms.PositionRoot);
		}
	}

	public Task ApplyReferencePose(EntityPose pose)
	{
		return _framework.RunOnFrameworkThread((Action)delegate
		{
			EntityPoseConverter entityPoseConverter = new EntityPoseConverter(pose);
			PoseContainer initial = entityPoseConverter.Save();
			entityPoseConverter.LoadReferencePose();
			PoseContainer final = entityPoseConverter.Save();
			_context.Actions.History.Add(new PoseMemento(entityPoseConverter)
			{
				Modes = PoseMode.All,
				Transforms = (PoseTransforms.Rotation | PoseTransforms.Position),
				Bones = null,
				Initial = initial,
				Final = final
			});
		});
	}

	public Task ApplyPartialReferencePose(EntityPose pose, int partialIndex)
	{
		return _framework.RunOnFrameworkThread((Action)delegate
		{
			EntityPoseConverter entityPoseConverter = new EntityPoseConverter(pose);
			PoseContainer initial = entityPoseConverter.Save();
			entityPoseConverter.LoadReferencePose(partialIndex);
			PoseContainer final = entityPoseConverter.Save();
			_context.Actions.History.Add(new PoseMemento(entityPoseConverter)
			{
				Modes = PoseMode.All,
				Transforms = (PoseTransforms.Rotation | PoseTransforms.Position),
				Bones = null,
				Initial = initial,
				Final = final
			});
		});
	}

	public Task ApplyPoseFile(EntityPose pose, PoseFile file, PoseMode modes = PoseMode.All, PoseTransforms transforms = PoseTransforms.Rotation, bool selectedBones = false, bool includeDescendants = false, bool anchorGroups = false, bool excludeEars = false)
	{
		return _framework.RunOnFrameworkThread((Action)delegate
		{
			if (file.Bones != null)
			{
				EntityPoseConverter entityPoseConverter = new EntityPoseConverter(pose);
				PoseContainer poseContainer = entityPoseConverter.Save();
				PoseContainer poseContainer2 = file.Bones;
				List<IMemento> list = new List<IMemento>();
				if (excludeEars)
				{
					poseContainer2 = entityPoseConverter.FilterExcludeBones(poseContainer2, PoseUtil.EarBones);
				}
				if (pose.HasDTFace() != file.HasDTFace())
				{
					poseContainer2 = entityPoseConverter.FilterExcludeBones(poseContainer2, new string[1] { "j_kao" });
					if (modes.HasFlag(PoseMode.Face))
					{
						modes ^= PoseMode.Face;
					}
				}
				if (selectedBones)
				{
					entityPoseConverter.LoadSelectedBones(poseContainer2, transforms, modes, includeDescendants);
					poseContainer2 = entityPoseConverter.Save(poseContainer2);
				}
				else
				{
					entityPoseConverter.Load(poseContainer2, modes, transforms);
				}
				list.Add(new PoseMemento(entityPoseConverter)
				{
					Modes = modes,
					Transforms = transforms,
					Bones = (selectedBones ? entityPoseConverter.GetSelectedBones(all: true, includeDescendants).ToList() : null),
					Initial = (selectedBones ? entityPoseConverter.FilterSelectedBones(poseContainer, all: true, includeDescendants) : poseContainer),
					Final = (selectedBones ? entityPoseConverter.FilterSelectedBones(poseContainer2, all: true, includeDescendants) : poseContainer2)
				});
				if (selectedBones && anchorGroups && transforms.HasFlag(PoseTransforms.Position))
				{
					List<PartialBoneInfo> bones = entityPoseConverter.GetSelectedBones(all: false, includeDescendants).ToList();
					entityPoseConverter.LoadBones(poseContainer, bones, PoseTransforms.Position, modes);
					list.Add(new PoseMemento(entityPoseConverter)
					{
						Modes = modes,
						Transforms = PoseTransforms.Position,
						Bones = bones,
						Initial = entityPoseConverter.FilterSelectedBones(poseContainer2, all: false, includeDescendants),
						Final = entityPoseConverter.FilterSelectedBones(poseContainer, all: false, includeDescendants)
					});
				}
				_context.Actions.History.Add(new MultipleMemento(list));
			}
		});
	}

	public Task<PoseFile> SavePoseFile(EntityPose pose)
	{
		return _framework.RunOnFrameworkThread<PoseFile>((Func<PoseFile>)(() => new EntityPoseConverter(pose).SaveFile()));
	}

	public Task StashPose(EntityPose pose)
	{
		return _framework.RunOnFrameworkThread((Action)delegate
		{
			PoseMode modes = PoseMode.All;
			PoseTransforms transforms = PoseTransforms.Rotation | PoseTransforms.Position;
			EntityPoseConverter entityPoseConverter = new EntityPoseConverter(pose);
			PoseContainer poseContainer = entityPoseConverter.Save();
			StashedPose = new PoseMemento(entityPoseConverter)
			{
				Modes = modes,
				Transforms = transforms,
				Bones = null,
				Initial = poseContainer,
				Final = poseContainer
			};
			StashedAt = DateTime.Now;
			StashedFrom = pose.Parent.Name;
		});
	}

	public Task ApplyStashedPose(EntityPose pose)
	{
		return _framework.RunOnFrameworkThread((Action)delegate
		{
			if (StashedPose != null)
			{
				EntityPoseConverter entityPoseConverter = new EntityPoseConverter(pose);
				PoseContainer initial = entityPoseConverter.Save();
				entityPoseConverter.Load(StashedPose.Final, StashedPose.Modes, StashedPose.Transforms);
				_context.Actions.History.Add(new PoseMemento(entityPoseConverter)
				{
					Modes = StashedPose.Modes,
					Transforms = StashedPose.Transforms,
					Bones = StashedPose.Bones,
					Initial = initial,
					Final = StashedPose.Final
				});
			}
		});
	}

	public Task ApplyFlipPose(EntityPose pose)
	{
		return _framework.RunOnFrameworkThread((Action)delegate
		{
			EntityPoseConverter entityPoseConverter = new EntityPoseConverter(pose);
			PoseContainer initial = entityPoseConverter.Save();
			entityPoseConverter.FlipPose();
			PoseContainer final = entityPoseConverter.Save();
			_context.Actions.History.Add(new PoseMemento(entityPoseConverter)
			{
				Modes = PoseMode.All,
				Transforms = (PoseTransforms.Rotation | PoseTransforms.Position),
				Bones = null,
				Initial = initial,
				Final = final
			});
		});
	}

	public void Dispose()
	{
		try
		{
			StashedPose = null;
			PoseModule?.Dispose();
			PoseModule = null;
			IkModule?.Dispose();
			IkModule = null;
			Attachments.Dispose();
			_context.Plugin.Config.OnSaved -= AutoSave.Configure;
			AutoSave.Dispose();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to dispose posing manager:\n{value}");
		}
		GC.SuppressFinalize(this);
	}
}
