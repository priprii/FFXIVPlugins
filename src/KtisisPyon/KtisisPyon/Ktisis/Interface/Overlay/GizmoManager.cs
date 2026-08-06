using System;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Ktisis.Data.Config;

namespace Ktisis.Interface.Overlay;

public class GizmoManager
{
	private readonly Configuration _cfg;

	private const string ImGuiVersion = "1.88";

	private bool IsInit;

	public GizmoManager(Configuration cfg)
	{
		_cfg = cfg;
	}

	public unsafe void Initialize()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (IsInit)
		{
			return;
		}
		bool flag = false;
		try
		{
			string version = ImGui.GetVersion();
			if (version != "1.88")
			{
				throw new Exception($"ImGui version mismatch! Expected {"1.88"}, got {version ?? "NULL"} instead.");
			}
			delegate*<nuint, void*, void*> delegate_002A = null;
			delegate*<void*, void*, void> delegate_002A2 = null;
			void* ptr = null;
			ImGui.GetAllocatorFunctions((delegate*<UIntPtr, void*, void*>*)(&delegate_002A), &delegate_002A2, &ptr);
			ImGuiContextPtr currentContext = ImGui.GetCurrentContext();
			delegate* unmanaged[Cdecl]<nuint, void*, void*> ptr2 = (delegate* unmanaged[Cdecl]<nuint, void*, void*>)delegate_002A;
			delegate* unmanaged[Cdecl]<void*, void*, void> ptr3 = (delegate* unmanaged[Cdecl]<void*, void*, void>)delegate_002A2;
			ImGuizmo.SetImGuiContext(ImGuiContextPtr.op_Implicit(currentContext.Handle));
			ImGuiMemAllocFunc delegateForFunctionPointer = Marshal.GetDelegateForFunctionPointer<ImGuiMemAllocFunc>((nint)ptr2);
			ImGuiMemFreeFunc delegateForFunctionPointer2 = Marshal.GetDelegateForFunctionPointer<ImGuiMemFreeFunc>((nint)ptr3);
			ImGui.SetAllocatorFunctions(delegateForFunctionPointer, delegateForFunctionPointer2, ptr);
			flag = true;
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to initialize gizmo:\n{value}");
		}
		Ktisis.Log.Verbose($"Completed gizmo init (success: {flag})");
		IsInit = flag;
	}

	public Gizmo Create(GizmoId id)
	{
		if (!IsInit)
		{
			throw new Exception("Can't create gizmo as ImGuizmo is not initialized.");
		}
		return new Gizmo(_cfg.Gizmo, id);
	}
}
