using UnityEngine;

public class SpawnMarker : MonoBehaviour
{
    [Header("References")]
    public Renderer markerRenderer;
    
    [Header("Materials")]
    public Material normalMaterial;   // 기본 상태 머터리얼
    public Material selectedMaterial; // 유닛이 선택(집어올려짐)되었을 때 머터리얼

    // 마커의 하이라이트 상태를 설정합니다.
    public void SetHighlight(bool isSelected)
    {
        if (markerRenderer == null) return;
        
        markerRenderer.material = isSelected ? selectedMaterial : normalMaterial;
    }
}
