using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LocalModelDatabase", menuName = "Database/Local Model Database", order = 1)]
public class LocalModelDatabase : ScriptableObject
{
    [System.Serializable]
    public class Model
    {
        public string ownerId;
        public string title;
        public string description;
        public List<string> tags;
        public string ldrUrl;
        public string previewImageUrl;
        public int brickCount;
        public bool isPublic;
        public int likes;
        public int downloads;
        public double createdAt;
        public double updatedAt;
    }

    public List<Model> models = new List<Model>();
} 