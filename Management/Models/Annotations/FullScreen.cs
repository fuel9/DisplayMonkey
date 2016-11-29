using DisplayMonkey.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    [
        MetadataType(typeof(FullScreen.FullScreenAnnotations))
    ]
    public partial class FullScreen : Panel
    {
        public FullScreen(DisplayMonkeyEntities _db, Canvas _canvas)
            : base(_db)
        {
            Left = 0;
            Top = 0;
            Height = _canvas.Height;
            Width = _canvas.Width;
            Name = Resources.FullScreen;
            Canvas = _canvas;

            init(_db);
        }
        
        public FullScreen() : 
            base() 
        {
        }

        protected override void init(DisplayMonkeyEntities _db)
        {
            Setting fadeLength = Setting.GetSetting(_db, Setting.Keys.DefaultFullPanelFadeLength);
            if (fadeLength != null)
            {
                this.FadeLength = fadeLength.DecimalValue;
            }
        }

        internal class FullScreenAnnotations
        {
            [
                Display(ResourceType = typeof(Resources), Name = "MaxIdleInterval"),
                /*Required(ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),*/
                Range(0, Int32.MaxValue,
                    ErrorMessageResourceType = typeof(Resources),
                    ErrorMessageResourceName = "PositiveIntegerRequired"),
            ]
            public Nullable<int> MaxIdleInterval { get; set; }
        }
    }
}