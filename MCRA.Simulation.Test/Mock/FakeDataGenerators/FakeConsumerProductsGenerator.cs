using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake foods
    /// </summary>
    public static class FakeConsumerProductsGenerator {

        private static string[] _defaultProducts = [
            "Eye liner", "Fard à paupières" ,"creme hydratante / de jour", "fond de teint crème",
            "fond de teint poudre compacte", "fond de teint poudre libre", "baume à lèvres",
            "creme nuit", "rouge à lèvres", "Mascara", "dentifrice", "Fard à joues / Blush",
            "creme hydratante", "huile hydratante", "lait hydratant", "Gommage / exfoliant",
            "Lait nettoyant", "Savon liquide", "Savon solide", "Tonique / Eau micellaire",
            "Crayon à yeux"
        ];

        /// <summary>
        /// Creates a list of products
        /// </summary>
        public static List<ConsumerProduct> MockProducts(params string[] names) {
            var result = names
                .Select(r => new ConsumerProduct() {
                    Code = r,
                    Name = r,
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates a list of products
        /// </summary>
        public static List<ConsumerProduct> Create(int n) {
            if (n <= _defaultProducts.Length) {
                var result = _defaultProducts
                    .Take(n)
                    .Select(r => new ConsumerProduct() {
                        Code = r,
                        Name = r,
                    })
                    .ToList();
                return result;
            }
            throw new Exception($"Cannot create more than {_defaultProducts.Length} mock consumer products using this method!");
        }
    }
}
