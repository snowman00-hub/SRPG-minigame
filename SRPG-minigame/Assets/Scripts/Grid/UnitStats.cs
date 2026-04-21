using UnityEngine;

public class UnitStats : MonoBehaviour
{
    [Header("Job Settings")]
    public JobData jobData;

    [Header("Runtime Info")]
    public UnitType unitType;
    public int gridX;
    public int gridZ;

    // Helper properties to access job data
    public int moveRange => jobData != null ? jobData.moveRange : 3;
    public int maxHeightDiff => jobData != null ? jobData.maxHeightDiff : 2;
    public string jobName => jobData != null ? jobData.jobName : "No Job";
}
