// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Web.Processors
{
    /// <summary>
    /// Allows the resizing of images.
    /// </summary>
    public class CropWebProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command constant for the crop proportions.
        /// </summary>
        public const string Crop = "crop";

        private static readonly IEnumerable<string> ResizeCommands
            = new[]
            {
                Crop
            };

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = ResizeCommands;

        /// <inheritdoc/>
        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands)
        {
            if (commands.ContainsKey(Crop))
            {
                var cropProportions = commands[Crop].Split(new char[] { ','});
                if (cropProportions.Length != 4)
                    return image;
                float left,top,right,bottom;
                if (!float.TryParse(cropProportions[0], out left)
                    || !float.TryParse(cropProportions[1], out top)
                    || !float.TryParse(cropProportions[2], out right)
                    || !float.TryParse(cropProportions[3], out bottom))
                    return image;
                var x = left * image.Image.Width;
                var y = top * image.Image.Height;
                var width = image.Image.Width - x - (right * image.Image.Width);
                var height = image.Image.Height - y - (bottom * image.Image.Height);
                var rect = new Rectangle((int)x, (int)y, (int)width, (int)height);
                image.Image.Mutate(m => m.Crop(rect));
                return image;
            }
            return image;
        }
    }
}