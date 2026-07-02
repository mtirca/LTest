using System;
using UnityEngine;

namespace Tools
{
    [Serializable]
    public class Label
    {
        public string id;
        public int sliceIndex;
        public string name;
        public string description;
        public Color color;
        public bool visible;
        public int rBandIndex;
        public int gBandIndex;
        public int bBandIndex;

        // ── Backend tracking fields ──────────────────────────────────────────
        /// <summary>
        /// Optimistic locking token returned by the backend on every save/load.
        /// Must be echoed back on the next POST to prevent 409 CONFLICT responses.
        /// </summary>
        public long version;

        /// <summary>
        /// Relative URL of the texture PNG stored on the backend (e.g. "/textures/abc.png").
        /// Set after a successful UploadLabel call; used by LoadSession to fetch the texture.
        /// </summary>
        public string textureUrl;

        public Label(string name, Color color, int sliceIndex, int rBandIndex, int gBandIndex, int bBandIndex)
        {
            id = Guid.NewGuid().ToString();
            visible = true;
            description = "";
            this.name = name;
            this.color = color;
            this.sliceIndex = sliceIndex;
            this.rBandIndex = rBandIndex;
            this.gBandIndex = gBandIndex;
            this.bBandIndex = bBandIndex;
        }
    }
}