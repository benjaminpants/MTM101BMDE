using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.PlusExtensions
{
    [Serializable]
    public class ExtendedPosterObject : PosterObject
    {
        public PosterImageData[] overlayData;
    }


    /// <summary>
    /// Used to store additional images to be overlayed ontop of text for posters.
    /// Can be used obscure text or other purposes.
    /// </summary>
    [Serializable]
    public class PosterImageData
    {
        public Texture2D texture;
        public IntVector2 position;
        public IntVector2 size;

        public PosterImageData(Texture2D texture, IntVector2 position)
        {
            this.texture = texture;
            this.position = position;
            size = new IntVector2(texture.width, texture.height);
        }

        public PosterImageData(Texture2D texture, IntVector2 position, IntVector2 size)
        {
            this.texture = texture;
            this.position = position;
            this.size = size;
        }

        public PosterImageData()
        {
            texture = null;
            position = new IntVector2(0,0);
            size = new IntVector2(0, 0);
        }
    }
}
