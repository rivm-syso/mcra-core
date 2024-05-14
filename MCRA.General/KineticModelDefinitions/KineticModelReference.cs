using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRA.General.KineticModelDefinitions {
    public class KineticModelReference {
        public string Id { get; set; }
        public string FileName { get; set; }
        public List<string> Aliases { get; set; }
    }
}
