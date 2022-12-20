namespace MCRA.Utils.Statistics.Histograms {

    /// <summary>
    /// Outlier handling method for histograms
    /// </summary>
    public enum OutlierHandlingMethod { 
        IncludeLower = -1, 
        IncludeNone = 0, 
        IncludeHigher = 1, 
        IncludeBoth = 2 
    }
}
