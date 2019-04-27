using UnityEditor;
using UnityEngine;

public class uGUITools : MonoBehaviour
{
	[MenuItem("uGUI/Anchors to Corners %[")]
	static void AnchorsToCorners()
	{
		foreach (Transform transform in Selection.transforms)
		{
			RectTransform t = transform as RectTransform;
			RectTransform pt = Selection.activeTransform.parent as RectTransform;

			if (t == null || pt == null) return;

			Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
												t.anchorMin.y + t.offsetMin.y / pt.rect.height);
			Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
												t.anchorMax.y + t.offsetMax.y / pt.rect.height);

			t.anchorMin = newAnchorsMin;
			t.anchorMax = newAnchorsMax;
			t.offsetMin = t.offsetMax = new Vector2(0, 0);
		}
	}

	[MenuItem("uGUI/Anchors to Pivot %`")]
	static void AnchorsToPivot()
	{
		foreach (Transform transform in Selection.transforms)
		{
			RectTransform t = transform as RectTransform;
			RectTransform pt = Selection.activeTransform.parent as RectTransform;

			if (t == null || pt == null) return;

			float oldWidth = t.rect.width;
			float oldHeight = t.rect.height;

			float anchorValueX = t.anchorMin.x + (t.offsetMin.x + (t.rect.width * t.pivot.x)) / pt.rect.width;
			float anchorValueY = t.anchorMin.y + (t.offsetMin.y + (t.rect.height * t.pivot.y)) / pt.rect.height;

			if (anchorValueX > 1)
			{
				anchorValueX = 1;
			}
			if (anchorValueY > 1)
			{
				anchorValueY = 1;
			}

			Vector2 anchorPoint = new Vector2(anchorValueX, anchorValueY);

			t.anchorMin = anchorPoint;
			t.anchorMax = anchorPoint;

			//Reset dimensions after anchor move
			//TODO: Account for an offset pivot here!
			t.offsetMin = new Vector2(-(oldWidth * t.pivot.x), -(oldHeight * t.pivot.y));
			t.offsetMax = new Vector2((oldWidth * (1 - t.pivot.x)), (oldHeight * (1 - t.pivot.y)));
		}
	}

	[MenuItem("uGUI/Corners to Anchors %]")]
	static void CornersToAnchors()
	{
		foreach (Transform transform in Selection.transforms)
		{
			RectTransform t = transform as RectTransform;

			if (t == null) return;

			t.offsetMin = t.offsetMax = new Vector2(0, 0);
		}
	}

	[MenuItem("uGUI/Mirror Horizontally Around Anchors %;")]
	static void MirrorHorizontallyAnchors()
	{
		MirrorHorizontally(false);
	}

	[MenuItem("uGUI/Mirror Horizontally Around Parent Center %:")]
	static void MirrorHorizontallyParent()
	{
		MirrorHorizontally(true);
	}

	static void MirrorHorizontally(bool mirrorAnchors)
	{
		foreach (Transform transform in Selection.transforms)
		{
			RectTransform t = transform as RectTransform;
			RectTransform pt = Selection.activeTransform.parent as RectTransform;

			if (t == null || pt == null) return;

			if (mirrorAnchors)
			{
				Vector2 oldAnchorMin = t.anchorMin;
				t.anchorMin = new Vector2(1 - t.anchorMax.x, t.anchorMin.y);
				t.anchorMax = new Vector2(1 - oldAnchorMin.x, t.anchorMax.y);
			}

			Vector2 oldOffsetMin = t.offsetMin;
			t.offsetMin = new Vector2(-t.offsetMax.x, t.offsetMin.y);
			t.offsetMax = new Vector2(-oldOffsetMin.x, t.offsetMax.y);

			t.localScale = new Vector3(-t.localScale.x, t.localScale.y, t.localScale.z);
		}
	}

	[MenuItem("uGUI/Mirror Vertically Around Anchors %'")]
	static void MirrorVerticallyAnchors()
	{
		MirrorVertically(false);
	}

	[MenuItem("uGUI/Mirror Vertically Around Parent Center %\"")]
	static void MirrorVerticallyParent()
	{
		MirrorVertically(true);
	}

	static void MirrorVertically(bool mirrorAnchors)
	{
		foreach (Transform transform in Selection.transforms)
		{
			RectTransform t = transform as RectTransform;
			RectTransform pt = Selection.activeTransform.parent as RectTransform;

			if (t == null || pt == null) return;

			if (mirrorAnchors)
			{
				Vector2 oldAnchorMin = t.anchorMin;
				t.anchorMin = new Vector2(t.anchorMin.x, 1 - t.anchorMax.y);
				t.anchorMax = new Vector2(t.anchorMax.x, 1 - oldAnchorMin.y);
			}

			Vector2 oldOffsetMin = t.offsetMin;
			t.offsetMin = new Vector2(t.offsetMin.x, -t.offsetMax.y);
			t.offsetMax = new Vector2(t.offsetMax.x, -oldOffsetMin.y);

			t.localScale = new Vector3(t.localScale.x, -t.localScale.y, t.localScale.z);
		}
	}
}