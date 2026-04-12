namespace Sounds
{
    public interface ISetVolume
    {
        public const string VOLUME_ID = "Volume";
        
        void SetVolume(float volume);
    }
}