
namespace Cruncher.Postprocessors.AutoPrefixer
{
    public class AutoPrefixerPostprocessor : IPostprocessor
    {
        public string[] AllowedExtensions
        {
            get
            {
                return new[] { ".CSS" };
            }
        }

        public string Transform(string input, string path, CruncherBase cruncher)
        {
            throw new System.NotImplementedException();
        }
    }
}
