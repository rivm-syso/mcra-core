using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum ClusterMethodType {
        [Display(Name = "Component selection (SNMU)")]
        [Description("Only component selection is performed.")]
        NoClustering,
        [Display(Name = "Component selection and population subgrouping (SNMU + k-means clustering)")]
        [Description("Component selection followed by K-Means clustering of individuals based on their component exposure. K-means classifies individuals in multiple groups (i.e., clusters), such that individuals within the same cluster are as similar as possible (i.e., high intra-class similarity), whereas individuals from different clusters are as dissimilar as possible (i.e., low inter-class similarity). In k-means clustering, each cluster is represented by its center (i.e, centroid) which corresponds to the mean of points assigned to the cluster.")]
        Kmeans,
        [Display(Name = "Component selection and population subgrouping (SNMU + hierarchical clustering)")]
        [Description("Component selection followed by hierarchical (Ward's) clustering of individuals based on their component exposure. Hierachical clustering builds a hierarchy from the bottom-up, and doesn’t require to specify the number of clusters beforehand. Hierarchical clustering produces a tree-based representation of the observations known as a dendrogram.")]
        Hierarchical,
    }
}
