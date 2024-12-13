using System.Drawing;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using Svg;
using Svg.DataTypes;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.AOPNetworks {
    public class AopNetworkGraphCreator : IReportChartCreator {

        public double BlockHeight { get; set; } = 30;
        public double BlockWidth { get; set; } = 125;
        public double VerticalMargin { get; set; } = 20;
        public double HorizontalMargin { get; set; } = 40;

        public int Height { get; set; }
        public int Width { get; set; }
        public bool ShowTitle { get; set; }

        private readonly SvgMarker arrowMarker = createArrowMarker();

        private readonly AopNetworkSummarySection _section;

        public string ChartId {
            get {
                var pictureId = "392328F6-AF7B-4171-8650-628B1D5C2FB2";
                return StringExtensions.CreateFingerprint(_section.SectionGuid + pictureId);
            }
        }

        public string Title {
            get {
                return $"{_section.AOPName} ({_section.AOPCode})";
            }
        }

        public AopNetworkGraphCreator() { }

        public AopNetworkGraphCreator(
            AopNetworkSummarySection section,
            int width = 900,
            bool showTitle = false
        ) {
            Width = width;
            ShowTitle = showTitle;
            _section = section;
        }

        public PlotModel Create() {
            throw new NotImplementedException();
        }

        public void CreateToPng(string filename) {
            throw new NotImplementedException();
        }

        public void CreateToSvg(string fileName) {
            Create(_section, fileName, ShowTitle ? Title : null);
        }

        public void WritePngToStream(Stream stream) {
            throw new NotImplementedException();
        }

        public string ToSvgString(int width, int height) {
            var doc = create(_section, ShowTitle ? Title : null);
            return doc.GetXML();
        }

        /// <summary>
        /// Creates an svg of the AOP network graph and writes it to the specified file.
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <param name="fileName"></param>
        public void Create(
            AopNetworkSummarySection aopNetwork,
            string fileName,
            string title = null
        ) {
            var doc = create(aopNetwork, title);
            doc.Write(fileName);
        }

        private SvgDocument create(AopNetworkSummarySection aopNetwork, string title) {
            var layers = getAopNetworkLayers(aopNetwork);

            var showTitle = !string.IsNullOrEmpty(title);
            var offsetX = 10D;
            var offsetY = showTitle ? 40D : 20D;

            var width = offsetX + layers.Count * (BlockWidth + HorizontalMargin) - HorizontalMargin + 1;
            var height = offsetY + layers.Max(r => r.KeyEvents.Count) * (BlockHeight + VerticalMargin) - VerticalMargin + 1;
            var doc = new SvgDocument() {
                Width = (float)(width),
                Height = (float)(height),
                FontSize = 10,
                FontFamily = "Arial",
            };
            var defsElement = new SvgDefinitionList() { ID = "defsMap" };
            doc.Children.Add(defsElement);
            defsElement.Children.Add(arrowMarker);

            if (showTitle) {
                var text = new SvgText() {
                    FontSize = 14,
                    FontWeight = SvgFontWeight.Bold,
                    Nodes = { new SvgContentNode() { Content = title } },
                    TextAnchor = SvgTextAnchor.Middle,
                    X = [0f],
                    Y = [0f],
                    Dx = [(float)width / 2f],
                    Fill = new SvgColourServer(Color.Black),
                };
                text.Dy = [2 * text.Bounds.Height];
                doc.Children.Add(text);
            }

            var keyEventNodes = drawKeyEvents(
                doc,
                layers,
                offsetX,
                offsetY
            );
            var kers = aopNetwork.KeyEventRelationships
                .Where(r => !r.IsIndirect)
                .Where(r => !r.IsCyclic)
                .ToList();
            drawKeyEventRelationships(doc, kers, keyEventNodes);
            doc.FlushStyles(true);
            return doc;
        }

        /// <summary>
        /// Draws the key event blocks and returns a dictionary in which the rectangle
        /// of the drawn block can be found for each key event can be found.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="layers"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        private Dictionary<string, SvgRectangle> drawKeyEvents(
            SvgDocument doc,
            IList<AopNetworkLayerRecord> layers,
            double offsetX,
            double offsetY
        ) {
            var nodeRects = new Dictionary<string, SvgRectangle>();
            var maxLayer = layers.Max(r => r.KeyEvents.Count);
            var x = offsetX;
            var previousBiologicalOrganisation = layers[0].BiologicalOrganisationLevel;
            for (int i = 0; i < layers.Count; i++) {
                var layer = layers[i];
                if (i > 0) {
                    var line = new SvgLine() {
                        StartX = (float)(x - HorizontalMargin / 2),
                        StartY = (float)offsetY,
                        EndX = (float)(x - HorizontalMargin / 2),
                        EndY = (float)(offsetY + maxLayer * (BlockHeight + VerticalMargin) - VerticalMargin),
                        Stroke = new SvgColourServer(Color.Black),
                        StrokeDashArray = [4f],
                    };
                    doc.Children.Add(line);
                }

                var y = offsetY + (maxLayer - layer.KeyEvents.Count) * (BlockHeight + VerticalMargin) / 2;

                var keyEvents = layer.KeyEvents.OrderBy(r => r.Code, StringComparer.OrdinalIgnoreCase).ToList();
                for (int j = 0; j < keyEvents.Count; j++) {
                    var keyEvent = keyEvents[j];
                    var group = new SvgGroup();
                    var rect = new SvgRectangle() {
                        X = new SvgUnit((float)x),
                        Y = new SvgUnit((float)y),
                        Width = new SvgUnit((float)BlockWidth),
                        Height = new SvgUnit((float)BlockHeight),
                        Stroke = new SvgColourServer(Color.Black),
                        Fill = new SvgColourServer(getKeyEventBlockColor(keyEvent.BiologicalOrganisationType)),
                        FillOpacity = 0.7f,
                        CornerRadiusX = 5f,
                        CornerRadiusY = 5f,
                    };
                    group.Children.Add(rect);
                    nodeRects.Add(keyEvent.Code, rect);

                    var text = new SvgText() {
                        Nodes = { new SvgContentNode() { Content = keyEvent.Name } },
                        TextAnchor = SvgTextAnchor.Middle,
                        X = [(float)x],
                        Y = [(float)y],
                        Dx = [(float)BlockWidth / 2f],
                        Fill = new SvgColourServer(Color.Black),
                    };
                    text.Dy = [(float)BlockHeight / 2f + text.Bounds.Height / 4f];
                    group.Children.Add(text);

                    doc.Children.Add(group);

                    previousBiologicalOrganisation = layer.BiologicalOrganisationLevel;
                    y += BlockHeight + VerticalMargin;
                }
                x += BlockWidth + HorizontalMargin;
            }
            return nodeRects;
        }

        /// <summary>
        /// Creates the key-event relationship connectors.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="kers"></param>
        /// <param name="keyEventNodes"></param>
        private void drawKeyEventRelationships(
            SvgDocument doc,
            ICollection<AopKeyEventRelationshipRecord> kers,
            Dictionary<string, SvgRectangle> keyEventNodes
        ) {
            foreach (var ker in kers) {
                var startNode = keyEventNodes[ker.CodeUpstreamKeyEvent];
                var endNode = keyEventNodes[ker.CodeDownstreamKeyEvent];
                var line = new SvgLine() {
                    StartX = startNode.X + startNode.Width,
                    StartY = startNode.Y + startNode.Height / 2,
                    EndX = endNode.X,
                    EndY = endNode.Y + startNode.Height / 2,
                    Stroke = new SvgColourServer(Color.Black),
                    MarkerEnd = new Uri($"url(#{arrowMarker.ID})", UriKind.Relative)
                };
                doc.Children.Add(line);
            }
        }

        private Color getKeyEventBlockColor(BiologicalOrganisationType biologicalOrganisation) {
            return biologicalOrganisation switch {
                BiologicalOrganisationType.Molecular => Color.Green,
                BiologicalOrganisationType.Cellular => Color.GreenYellow,
                BiologicalOrganisationType.Organ => Color.Orange,
                BiologicalOrganisationType.Individual => Color.Red,
                BiologicalOrganisationType.Population => Color.Red,
                _ => Color.LightGray,
            };
        }

        private static SvgMarker createArrowMarker() {
            var size = 10f;
            return new SvgMarker() {
                ID = "markerArrow",
                RefX = size * 1f,
                RefY = size * .5f,
                MarkerUnits = SvgMarkerUnits.StrokeWidth,
                MarkerWidth = size,
                MarkerHeight = size,
                Orient = new SvgOrient() { IsAuto = true },
                Children = {
                new SvgPath() {
                    ID = "pathMarkerArrow",
                    Fill = new SvgColourServer(Color.Black),
                    PathData = SvgPathBuilder.Parse($@"M 0 0 L {size * 1f} {size * .5f} L 0 {size} z".AsSpan())
                }
            }
            };
        }

        /// <summary>
        /// Gets sequential layers of the AOP network.
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <returns></returns>
        private static IList<AopNetworkLayerRecord> getAopNetworkLayers(
            AopNetworkSummarySection aopNetwork
        ) {
            var kers = aopNetwork.KeyEventRelationships.Where(r => !r.IsCyclic).ToList();

            var keyEventsDict = aopNetwork.KeyEvents.ToDictionary(r => r.Code);
            var toNodesLookup = kers.ToLookup(r => r.CodeDownstreamKeyEvent, r => keyEventsDict[r.CodeUpstreamKeyEvent]);
            var fromNodesLookup = kers.ToLookup(r => r.CodeUpstreamKeyEvent, r => keyEventsDict[r.CodeDownstreamKeyEvent]);
            var rootNodes = aopNetwork.KeyEvents
                .Where(r => !toNodesLookup.Contains(r.Code))
                .ToList();
            var result = getAopNetworkLayersRecursive(
                rootNodes,
                toNodesLookup,
                fromNodesLookup,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            );
            return result;
        }

        private static IList<AopNetworkLayerRecord> getAopNetworkLayersRecursive(
            ICollection<AopKeyEventRecord> currentNodes,
            ILookup<string, AopKeyEventRecord> toNodesLookup,
            ILookup<string, AopKeyEventRecord> fromNodesLookup,
            HashSet<string> visited
        ) {
            var layers = new List<AopNetworkLayerRecord>();

            var biologicalOrganisations = currentNodes
                .Select(r => r.BiologicalOrganisationType)
                .Distinct()
                .ToList();
            var processBiologicalOrganisationLevel = biologicalOrganisations.Min();
            var processNodes = (biologicalOrganisations.Count > 1)
                ? currentNodes.Where(r => r.BiologicalOrganisationType == processBiologicalOrganisationLevel).ToList()
                : currentNodes;

            layers.Add(new AopNetworkLayerRecord() {
                KeyEvents = processNodes,
                BiologicalOrganisationLevel = (int)processNodes.First().BiologicalOrganisationType
            });
            foreach (var node in processNodes) {
                visited.Add(node.Code);
            }
            var postPonedNodes = currentNodes.Except(processNodes).ToList();
            var nextLayerNodes = processNodes
                .SelectMany(r => fromNodesLookup[r.Code])
                .Distinct()
                .Where(r => toNodesLookup[r.Code].All(n => r == n || visited.Contains(n.Code)))
                .ToList();
            nextLayerNodes.AddRange(postPonedNodes);

            if (nextLayerNodes.Any()) {
                layers.AddRange(getAopNetworkLayersRecursive(nextLayerNodes, toNodesLookup, fromNodesLookup, visited));
            }
            return layers;
        }
    }
}
