using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNovelManagerv2.Design
{
    public class AddVnViewModelService
    {
        public string PickedFileName { get; set; }
        public Action FilePicked { get; set; }
    }
}
