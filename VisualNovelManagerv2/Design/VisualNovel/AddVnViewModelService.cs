using System;

namespace VisualNovelManagerv2.Design.VisualNovel
{
    public class AddVnViewModelService
    {
        public string PickedFileName { get; set; }
        public Action FilePicked { get; set; }
    }
}
