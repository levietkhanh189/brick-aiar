using System.Threading.Tasks;
using UnityEngine;

public class LDRController : MonoBehaviour
{
    [Sirenix.OdinInspector.Button("Get LDR")]
    public async Task<string> GetLDR(string ldrUrl)
    {
        return await LDRFileManager.Instance.GetLDRFile(ldrUrl);
    }
}
