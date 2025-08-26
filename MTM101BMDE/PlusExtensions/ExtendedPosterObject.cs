using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MTM101BaldAPI.PlusExtensions
{

    /// <summary>
    /// An extended version of the PosterObject that adds extra functionality.
    /// </summary>
    [Serializable]
    public class ExtendedPosterObject : PosterObject
    {
        public PosterImageData[] overlayData;

        public ExtendedPosterObject()
        {
            testMaterial = null;
        }
    }

    /// <summary>
    /// An extended version of PosterTextData that adds extra functionality useful for automating poster creation or reducing copy and pasted localization.
    /// Must be used under an ExtendedPosterObject.
    /// </summary>
    [Serializable]
    public class ExtendedPosterTextData : PosterTextData
    {
        /// <summary>
        /// Calls string.Format with the specified localized strings
        /// </summary>
        public string[] formats = new string[0];
        /// <summary>
        /// Replacement regex, where the first string is the regex itself and the second is the replacement
        /// </summary>
        public string[][] replacementRegex = new string[0][];
    }


    /// <summary>
    /// Used to store additional images to be overlayed ontop of text for posters.
    /// Can be used to obscure text or other purposes.
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
