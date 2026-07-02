using System;
using UnityEngine;

namespace API
{
    /// <summary>
    /// Mirrors the ArtifactLabel JSON structure returned by the Spring Boot backend.
    /// Used exclusively for deserializing GET /api/labels/{artifactId} responses.
    /// Unity-side concerns (textureUrl, version, artifactId) live here, not in Label.cs.
    /// </summary>
    [Serializable]
    public class ArtifactLabelDto
    {
        public string id;
        public string artifactId;
        public string name;
        public string description;
        public int sliceIndex;
        public bool visible;
        public int rBandIndex;
        public int gBandIndex;
        public int bBandIndex;

        /// <summary>Relative URL to the texture PNG (e.g. "/textures/abc123.png"), set by the backend.</summary>
        public string textureUrl;

        /// <summary>Optimistic locking token. Must be echoed back on every POST update.</summary>
        public long version;

        public ArtifactLabelColor color;

        /// <summary>
        /// Nested RGBA color — mirrors the ArtifactLabel.UnityColor embedded class on the backend.
        /// </summary>
        [Serializable]
        public class ArtifactLabelColor
        {
            public float r;
            public float g;
            public float b;
            public float a;

            public Color ToUnityColor() => new Color(r, g, b, a);
        }
    }

    /// <summary>
    /// Wrapper used to deserialize the JSON array returned by GET /api/labels/{artifactId}
    /// via JsonUtility, which requires a root object.
    /// </summary>
    [Serializable]
    public class ArtifactLabelDtoList
    {
        public ArtifactLabelDto[] items;
    }
}
