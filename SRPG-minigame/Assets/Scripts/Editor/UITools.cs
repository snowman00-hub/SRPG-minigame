using UnityEngine;
using UnityEditor;

/// <summary>
/// UI 작업을 편리하게 도와주는 에디터 유틸리티입니다.
/// Alt + 1: 선택한 UI의 앵커를 현재 크기에 맞춰 꼭짓점으로 이동 (Stretch)
/// Alt + 2: 선택한 UI의 피벗을 중앙(0.5, 0.5)으로 설정 (위치 유지)
/// </summary>
public class UITools : Editor
{
    [MenuItem("Tools/UI/Anchors to Corners %q")]
    public static void AnchorsToCorners()
    {
        GameObject[] selections = Selection.gameObjects;
        if (selections == null || selections.Length == 0)
        {
            Debug.LogWarning("UI 오브젝트를 선택해주세요.");
            return;
        }

        foreach (GameObject go in selections)
        {
            RectTransform t = go.GetComponent<RectTransform>();
            RectTransform p = go.transform.parent as RectTransform;

            if (t == null || p == null) continue;

            Undo.RecordObject(t, "Anchors to Corners");

            // 부모의 크기 대비 현재 UI의 상대적 위치를 계산하여 앵커 설정
            Vector2 newAnchMin = new Vector2(t.anchorMin.x + t.offsetMin.x / p.rect.width,
                                             t.anchorMin.y + t.offsetMin.y / p.rect.height);
            Vector2 newAnchMax = new Vector2(t.anchorMax.x + t.offsetMax.x / p.rect.width,
                                             t.anchorMax.y + t.offsetMax.y / p.rect.height);

            t.anchorMin = newAnchMin;
            t.anchorMax = newAnchMax;
            
            // Offset을 0으로 만들어 앵커에 딱 붙게 설정
            t.offsetMin = t.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// UI의 실제 위치를 유지하면서 피벗만 변경하는 유틸리티 함수입니다.
    /// </summary>
    private static void SetPivot(RectTransform rectTransform, Vector2 pivot)
    {
        Vector2 size = rectTransform.rect.size;
        Vector2 deltaPivot = rectTransform.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        
        rectTransform.pivot = pivot;
        rectTransform.localPosition -= rectTransform.localRotation * Vector3.Scale(deltaPosition, rectTransform.localScale);
    }
}
