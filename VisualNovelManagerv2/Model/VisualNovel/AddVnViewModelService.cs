using System;

namespace VisualNovelManagerv2.Model.VisualNovel
{
    public class AddVnViewModelService
    {
        public string PickedFileName { get; set; }
        public Action FilePicked { get; set; }
    }
}
