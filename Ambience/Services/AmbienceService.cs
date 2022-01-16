using Microsoft.Extensions.Options;

namespace Ambience.Services
{
    public class AmbienceService
    {
        private readonly List<Domain.Ambience>? _ambiences;

        public AmbienceService(IOptionsSnapshot<AmbienceServiceOptions> options)
        {
            _ambiences = options.Value.AmbienceList?.ToList();
        }

        public Domain.Ambience? GetAmbience(string Key)
        {
            if (_ambiences != null)
            {
                var ambience = _ambiences.FirstOrDefault(x => x?.Name == Key, null);

                if (ambience != null)
                    return ambience;
            }

            return null;
        }

        public List<Domain.Ambience>? GetAmbiences()
        {
            return _ambiences;
        }
    }
}
