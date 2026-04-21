using UnityEngine;

[CreateAssetMenu(fileName = "NewJobData", menuName = "SRPG/Job Data")]
public class JobData : ScriptableObject
{
    public string jobName;
    public int moveRange = 3;
    public int maxHeightDiff = 2;
    public Color jobColor = Color.white;
}
