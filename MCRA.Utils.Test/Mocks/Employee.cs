namespace MCRA.Utils.Test.Mocks {
    public class Employee {

        public string Name { get; set; }

        public int Age { get; set; }

        public string Description {
            get { return "Medewerker"; }
        }

        public string Country { get; set; }

        public override bool Equals(object obj) {
            var emp = obj as Employee;
            return (emp != null && emp.Name == this.Name && emp.Age == this.Age);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            return $"Name: {Name}, Age: {Age}";
        }
    }
}
