using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class LegoController : MonoBehaviour
{
    public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    public float radius = 1f;
    public float duration = 1f;
    public Ease easeType = Ease.OutQuad;

    void Start()
    {
        //FlyInGatherEffect(radius, duration, easeType);
    }

    public void Init(Material material)
    {
        ChangeColor(material);
    }

    public void ChangeColor(Material material)
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
    }

    public void FlyInGatherEffect(float radius, float duration, Ease easeType = Ease.OutQuad)
    {
        // Lấy vị trí ban đầu của đối tượng
        Vector3 originalPosition = transform.position;
        
        // Tạo một vị trí ngẫu nhiên trong bán kính radius xung quanh vị trí ban đầu
        Vector3 startPosition = originalPosition * radius;
        transform.position = startPosition;
        
        // Sử dụng DOTween để tạo animation di chuyển về vị trí gốc
        transform.DOMove(originalPosition, duration).SetEase(easeType);
    }
}