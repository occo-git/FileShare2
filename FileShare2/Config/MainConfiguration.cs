public class MainConfiguration
{
    public long MaxFileSize { get; private set; }
    public int PixelsPerModule { get; private set; }

    public MainConfiguration(long maxFileSize, int pixelsPerModule) 
    { 
        MaxFileSize = maxFileSize;
        PixelsPerModule = pixelsPerModule;
    }
}