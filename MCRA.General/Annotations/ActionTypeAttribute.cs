using System;
using System.Linq;

namespace MCRA.General.Annotations {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ActionTypeAttribute : Attribute {

        public ActionType ActionType { get; private set; }

        public ActionTypeAttribute(ActionType actionType) {
            this.ActionType = actionType;
        }

        public static ActionType GetActionType(Type t) {
            var taskTypeAttribute = t.GetCustomAttributes(typeof(ActionTypeAttribute), false).FirstOrDefault() as ActionTypeAttribute;
            if (taskTypeAttribute != null) {
                return taskTypeAttribute.ActionType;
            }
            throw new Exception("Undefined action type");
        }
    }
}
