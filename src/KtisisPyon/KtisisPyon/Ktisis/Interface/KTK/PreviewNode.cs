using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Extensions;
using KamiToolKit.Nodes;
using KamiToolKit.Overlay.UiOverlay;
using Ktisis.Data.Files;
using Ktisis.Data.Json;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Data;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Factory.Builders;

namespace Ktisis.Interface.KTK;

public class PreviewNode : OverlayNode
{
	private readonly ImageNode Image;

	private readonly ImageNode ImageBacking;

	private readonly NineGridNode Border;

	private readonly NodeBase Buttons;

	private uint _counter;

	private ActorEntity _actor;

	private ActorEntity _target;

	private bool needsToApplyCollection = true;

	private unsafe readonly RenderTargetManager* _renderTargetManager;

	private unsafe readonly AgentInspect* _agentInspect;

	private ImGuiWindowPtr _fileWindow;

	private readonly IFramework _framework;

	private readonly IObjectTable _objectTable;

	private readonly IEditorContext _ctx;

	private readonly JsonFileSerializer _serializer;

	private PoseFile? _currentPose;

	private PoseTransforms _currentTransforms;

	private PoseMode _currentMode;

	private bool _currentEars;

	private bool _currentAnchor;

	private bool _currentBones;

	private bool _currentChildren;

	public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;

	public override bool HideWithNativeUi => false;

	public override bool IsVisible { get; set; }

	protected override void OnUpdate()
	{
	}

	public unsafe PreviewNode(IEditorContext context, IFramework framework, IObjectTable objectTable, ActorEntity target)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Expected O, but got Unknown
		PreviewNode previewNode = this;
		if (target.GetHuman() != null)
		{
			_target = target;
			_currentPose = null;
			_framework = framework;
			_objectTable = objectTable;
			_counter = 1u;
			_fileWindow = ImGuiWindowPtr.op_Implicit((ImGuiWindow*)null);
			_ctx = context;
			_serializer = new JsonFileSerializer();
			_renderTargetManager = RenderTargetManager.Instance();
			_agentInspect = AgentInspect.Instance();
			Image = new ImageNode
			{
				Size = new Vector2(192f, 320f),
				Position = new Vector2(4f, 3f),
				ImageNodeFlags = (ImageNodeFlags)140,
				WrapMode = WrapMode.Tile
			};
			ImageBacking = new ImageNode
			{
				Size = new Vector2(192f, 320f),
				Position = new Vector2(4f, 3f),
				ImageNodeFlags = (ImageNodeFlags)128,
				WrapMode = WrapMode.Tile
			};
			Border = new NineGridNode
			{
				Size = new Vector2(200f, 328f),
				TopOffset = 14f,
				LeftOffset = 14f,
				RightOffset = 14f,
				BottomOffset = 14f
			};
			Border.AddPart(new Part
			{
				TexturePath = "ui/uld/PreviewA_hr1.tex",
				Size = new Vector2(36f, 36f),
				TextureCoordinates = new Vector2(0f, 0f),
				Id = 0u
			});
			(*Image.AddPart(new Part
			{
				Height = 320f,
				Width = 192f
			})).LoadTexture(Pointer<Texture>.op_Implicit(((RenderTargetManager)_renderTargetManager).CharaViewTextures[1]));
			((Texture)((RenderTargetManager)_renderTargetManager).CharaViewTextures[1].Value).IncRef();
			(*ImageBacking.AddPart(new Part
			{
				Height = 320f,
				Width = 192f
			})).LoadTexture("ui/common/characterbg_hr1.tex");
			_framework.RunOnFrameworkThread((Action)delegate
			{
				((InspectCharaView)(&((AgentInspect)previewNode._agentInspect).CharaView)).Initialize(&((AgentInspect)previewNode._agentInspect).AgentInterface, 1u, (IntPtr)0);
				((CharaViewModelData)(&((InspectCharaView)(&((AgentInspect)previewNode._agentInspect).CharaView)).ModelData)).CopyFromCharacter((Character*)target.Actor.Address);
			});
			Buttons = SetupButtons();
			_actor = new ActorEntity(_ctx.Scene, new PoseBuilder(_ctx.Scene), _objectTable[441]);
			_actor.Setup();
			_framework.Update += new OnUpdateDelegate(OnFramework);
			ImageBacking.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
			Image.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
			Border.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
			Buttons.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
			((InspectCharaView)(&((AgentInspect)_agentInspect).CharaView)).Update(_counter, ((InspectCharaView)(&((AgentInspect)_agentInspect).CharaView)).GetCharacter());
		}
	}

	private unsafe void OnFramework(IFramework framework)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		((InspectCharaView)(&((AgentInspect)_agentInspect).CharaView)).Update(_counter, _actor.Character);
		((InspectCharaView)(&((AgentInspect)_agentInspect).CharaView)).Render(_counter++);
		_fileWindow = ImGuiP.FindWindowByName(ImU8String.op_Implicit("###OpenFileDialog"));
		if (!_ctx.Plugin.Gui.FileDialogs.IsDialogOpen())
		{
			Cleanup();
			return;
		}
		IsVisible = true;
		Position = new Vector2(((ImGuiWindowPtr)(ref _fileWindow)).Pos.X + ((ImGuiWindowPtr)(ref _fileWindow)).Size.X, ((ImGuiWindowPtr)(ref _fileWindow)).Pos.Y);
		if (NeedsUpdate() && _currentPose != null)
		{
			_ctx.Posing.ApplyReferencePose(_actor.Pose);
			if (_ctx.Config.File.ImportPoseSelectedBones)
			{
				CopySelectedBones();
			}
			if (!_actor.Pose.HasDTFace())
			{
				_actor.Pose.Update();
			}
			ApplyPose();
			UpdateLocals();
		}
	}

	private NodeBase SetupButtons()
	{
		NodeBase nodeBase = new ResNode
		{
			Size = new Vector2(168f, 32f),
			Position = new Vector2(8f, 286f),
			Priority = 1
		};
		ButtonBase buttonBase = new CircleButtonNode
		{
			Icon = ButtonIcon.RightArrow,
			Position = new Vector2(64f, 0f),
			Size = new Vector2(32f, 32f),
			Scale = new Vector2(-1f, 1f)
		};
		buttonBase.OnClick = delegate
		{
			MoveCamera(0f, -50f);
		};
		ButtonBase buttonBase2 = new CircleButtonNode
		{
			Icon = ButtonIcon.RightArrow,
			Position = new Vector2(64f, 0f),
			Size = new Vector2(32f, 32f)
		};
		buttonBase2.OnClick = delegate
		{
			MoveCamera(0f, 50f);
		};
		CircleButtonNode obj = new CircleButtonNode
		{
			Icon = ButtonIcon.Undo,
			Position = new Vector2(148f, 0f),
			Size = new Vector2(32f, 32f),
			OnClick = ResetCamera
		};
		buttonBase.AttachNode(nodeBase);
		buttonBase2.AttachNode(nodeBase);
		obj.AttachNode(nodeBase);
		return nodeBase;
	}

	private unsafe void MoveCamera(float pitch, float yaw)
	{
		((InspectCharaView)(&((AgentInspect)_agentInspect).CharaView)).SetCameraYawAndPitch(yaw, pitch);
	}

	private unsafe void ResetCamera()
	{
		((InspectCharaView)(&((AgentInspect)_agentInspect).CharaView)).ResetPositions();
	}

	public void PoseActor(string path)
	{
		string text = File.ReadAllText(path);
		if (Path.GetExtension(path).Equals(".cmp"))
		{
			text = LegacyPoseHelpers.ConvertLegacyPose(text);
		}
		_currentPose = _serializer.Deserialize<PoseFile>(text);
		_ctx.Posing.ApplyReferencePose(_actor.Pose);
		if (_ctx.Config.File.ImportPoseSelectedBones)
		{
			CopySelectedBones();
		}
		if (!_actor.Pose.HasDTFace())
		{
			_actor.Pose.Update();
		}
		ApplyPose();
		UpdateLocals();
	}

	private void ApplyPose()
	{
		_ctx.Posing.ApplyPoseFile(_actor.Pose, _currentPose, PoseMode.Body, _ctx.Config.File.ImportPoseTransforms, anchorGroups: _ctx.Config.File.AnchorPoseSelectedBones, selectedBones: _target.Pose.Recurse().Any((SceneEntity b) => b.IsSelected) && _ctx.Config.File.ImportPoseSelectedBones, includeDescendants: _ctx.Config.File.SelectedBonesIncludeDescendants, excludeEars: _ctx.Config.File.ExcludePoseEarBones);
	}

	private void UpdateLocals()
	{
		_currentAnchor = _ctx.Config.File.AnchorPoseSelectedBones;
		_currentBones = _ctx.Config.File.ImportPoseSelectedBones;
		_currentAnchor = _ctx.Config.File.AnchorPoseSelectedBones;
		_currentChildren = _ctx.Config.File.SelectedBonesIncludeDescendants;
		_currentEars = _ctx.Config.File.ExcludePoseEarBones;
		_currentMode = _ctx.Config.File.ImportPoseModes;
		_currentTransforms = _ctx.Config.File.ImportPoseTransforms;
	}

	private void CopySelectedBones()
	{
		foreach (SkeletonNode item in (from entity in _actor.Pose.Recurse().Prepend(_actor)
			where entity is SkeletonNode && entity.IsSelected
			select entity).Cast<SkeletonNode>())
		{
			_ctx.Selection.Unselect(item);
		}
		IEnumerable<SkeletonNode> nodes = (from entity in _target.Pose.Recurse().Prepend(_target)
			where entity is SkeletonNode && entity.IsSelected
			select entity).Cast<SkeletonNode>();
		IEnumerable<PartialBoneInfo> enumerable = GetBoneSelectionFrom(nodes).Distinct();
		if (_ctx.Config.File.SelectedBonesIncludeDescendants)
		{
			enumerable = _target.Pose.ExpandToDescendants(enumerable);
		}
		foreach (PartialBoneInfo item2 in enumerable)
		{
			_actor.Pose.FindBoneByName(item2.Name)?.Select();
		}
	}

	private IEnumerable<PartialBoneInfo> GetBoneSelectionFrom(IEnumerable<SkeletonNode> nodes, bool all = true)
	{
		foreach (SkeletonNode node in nodes)
		{
			if (!(node is BoneNode boneNode))
			{
				if (!(node is SkeletonGroup skeletonGroup))
				{
					continue;
				}
				foreach (PartialBoneInfo item in GetBoneSelectionFrom(all ? skeletonGroup.GetAllBones() : skeletonGroup.GetIndividualBones()))
				{
					yield return item;
				}
			}
			else
			{
				yield return boneNode.Info;
			}
		}
	}

	private bool NeedsUpdate()
	{
		if (_currentAnchor == _ctx.Config.File.AnchorPoseSelectedBones && _currentBones == _ctx.Config.File.ImportPoseSelectedBones && _currentAnchor == _ctx.Config.File.AnchorPoseSelectedBones && _currentChildren == _ctx.Config.File.SelectedBonesIncludeDescendants && _currentEars == _ctx.Config.File.ExcludePoseEarBones && _currentMode == _ctx.Config.File.ImportPoseModes)
		{
			return _currentTransforms != _ctx.Config.File.ImportPoseTransforms;
		}
		return true;
	}

	public unsafe void Cleanup()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		_framework.Update -= new OnUpdateDelegate(OnFramework);
		((InspectCharaView)(&((AgentInspect)_agentInspect).CharaView)).Release();
		Dispose();
	}
}
