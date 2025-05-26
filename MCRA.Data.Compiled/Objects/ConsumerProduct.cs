namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProduct : StrongEntity {

        public ConsumerProduct() {
            Children = [];
        }

        public ConsumerProduct(string code) : this() {
            Code = code;
        }

        public ConsumerProduct Parent { get; set; }
        public ICollection<ConsumerProduct> Children { get; set; }

    }
}
