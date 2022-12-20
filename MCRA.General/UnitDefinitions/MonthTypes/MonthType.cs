using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum MonthType {
        [Description("Undefined")]
        [Display(Name = "Undefined", ShortName = "Undefined")]
        Undefined = -1,
        [Description("January")]
        [Display(Name = "January", ShortName = "Jan")]
        January = 1,
        [Description("February")]
        [Display(Name = "February", ShortName = "Feb")]
        February = 2,
        [Description("March")]
        [Display(Name = "March", ShortName = "Mar")]
        March = 3,
        [Description("April")]
        [Display(Name = "April", ShortName = "Apr")]
        April = 4,
        [Description("May")]
        [Display(Name = "May", ShortName = "May")]
        May = 5,
        [Description("June")]
        [Display(Name = "June", ShortName = "Jun")]
        June = 6,
        [Description("July")]
        [Display(Name = "July", ShortName = "Jul")]
        July = 7,
        [Description("August")]
        [Display(Name = "August", ShortName = "Aug")]
        August = 8,
        [Description("September")]
        [Display(Name = "September", ShortName = "Sep")]
        September = 9,
        [Description("October")]
        [Display(Name = "October", ShortName = "Oct")]
        October = 10,
        [Description("November")]
        [Display(Name = "November", ShortName = "Nov")]
        November = 11,
        [Description("December")]
        [Display(Name = "December", ShortName = "Dec")]
        December = 12,
    }
}
