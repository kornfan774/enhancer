using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace XT.Base {

internal static class Host {

public static string projectPath = Directory.GetCurrentDirectory();
public static string settingsPath = projectPath + "/ProjectSettings";
public static string assetsPath = Application.dataPath;
public static bool darkEnabled => EditorGUIUtility.isProSkin;

public static object mainView => WindowLayoutR.FindMainView();
public static EditorWindow[] windows => Resources.FindObjectsOfTypeAll<EditorWindow>();
public static Rect position => ViewR.Wrap(WindowLayoutR.FindMainView()).screenPosition;

public static event Action reflowEvent;

public static EditorWindow GetWindow(Type type) {
	UObject[] windows = Resources.FindObjectsOfTypeAll(type);
	return windows.Length > 0 ? windows[0] as EditorWindow: null;
}

public static T GetWindow<T>() where T : EditorWindow {
	T[] windows = Resources.FindObjectsOfTypeAll<T>();
	return windows.Length > 0 ? windows[0] : null;
}

public static void OnReflow() {
	reflowEvent?.Invoke();
}

public static bool DockWindowLeft(EditorWindow window) {
	EditorWindow mostLeft = GetMostLeftWindow(window);
	if (mostLeft == null) {
		return false;
	}
	object parent = mostLeft.GetParent();
	if (parent == null) {
		return false;
	}
	// force the after-dock reflow to respect the current window widths
	EditorWindow[] windows = Host.windows;
	Vector2[] minSizes = new Vector2[windows.Length];
	for (int i = 0; i < minSizes.Length; i++) {
		minSizes[i] = windows[i].minSize;
		Vector2 size = windows[i].minSize;
		size.x = windows[i].position.size.x;
		windows[i].minSize = size;
	}
	// dock the window
	Rect rect = ViewR.Wrap(parent).screenPosition;
	DockWindow(window, mostLeft, rect.position);
	// restore min sizes
	for (int i = 0; i < windows.Length; i++) {
		windows[i].minSize = minSizes[i];
	}
	return true;
}

public static EditorWindow GetMostLeftWindow(EditorWindow except = null) {
	foreach (var window in windows) {
		if (window != except && window != null && !window.IsFloating()) {
			Rect p = window.GetWindowPosition();
			if (p.x == 0) {
				return window;
			}
		}
	}
	return null;
}

public static bool DockWindowRight(EditorWindow window) {
	EditorWindow mostRight = GetMostRightWindow(window);
	if (mostRight == null) {
		return false;
	}
	object parent = mostRight.GetParent();
	if (parent == null) {
		return false;
	}
	// force the after-dock reflow to respect the current window widths
	EditorWindow[] windows = Host.windows;
	Vector2[] minSizes = new Vector2[windows.Length];
	for (int i = 0; i < minSizes.Length; i++) {
		EditorWindow w = windows[i];
		minSizes[i] = w.minSize;
		w.minSize = w.position.size;
	}
	// dock the window
	Rect rect = ViewR.Wrap(parent).screenPosition;
	Vector2 screenPoint = new Vector2(rect.x + rect.width, rect.y);
	DockWindow(window, mostRight, screenPoint);
	// restore min sizes
	for (int i = 0; i < windows.Length; i++) {
		windows[i].minSize = minSizes[i];
	}
	return true;
}

static EditorWindow GetMostRightWindow(EditorWindow except) {
	float mainWidth = position.width;
	foreach (var window in windows) {
		if (window != except && window != null && !window.IsFloating()) {
			Rect p = window.GetWindowPosition();
			if (p.x + p.width == mainWidth) {
				return window;
			}
		}
	}
	return null;
}

static void DockWindow(EditorWindow window, EditorWindow anchor, Vector2 screenPoint) {
	object dockArea = anchor.GetParent();
	object containerWindow = DockAreaR.Wrap(dockArea).window;
	object rootSplitView = ContainerWindowR.Wrap(containerWindow).rootSplitView;
	DockAreaR.dragSource = window.GetParent();
	object dropInfo = SplitViewR.Wrap(rootSplitView).DragOverRootView(screenPoint);
	if (dropInfo == null || DropInfoR.Wrap(dropInfo).dropArea == null) {
		return;
	}
	SplitViewR.Wrap(rootSplitView).PerformDrop(window, dropInfo, screenPoint);
	OnReflow();
}

}

}