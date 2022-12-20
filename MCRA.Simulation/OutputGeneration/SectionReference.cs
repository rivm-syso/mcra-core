using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using System;

namespace MCRA.Simulation.OutputGeneration {
    public class SectionReference {
        public Guid SectionId { get; set; }
        public string Title { get; set; }
        public SectionReference(Guid sectionId, string title) {
            SectionId = sectionId;
            Title = title;
        }

        public static SectionReference FromHeader(SectionHeader header, string title = null) {
            return header != null
                ? new SectionReference(header.SectionId, title ?? header.Name)
                : new SectionReference(Guid.Empty, title);
        }

        public static SectionReference FromActionTypeSettings(SummaryToc toc, ActionType actionType, string title = null) {
            var header = toc?.GetSubSectionHeaderFromTitleString<SettingsSummarySection>(actionType.GetDisplayName());
            return header != null
                ? FromHeader(header, title)
                : new SectionReference(Guid.Empty, title);
        }
    }
}
