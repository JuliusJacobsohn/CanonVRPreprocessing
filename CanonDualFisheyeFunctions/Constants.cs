namespace CanonDualFisheyeFunctions
{
    public class Constants
    {
        public const int CANON_PICTURE_SOURCE_WIDTH = 8192;
        public const int CANON_PICTURE_SOURCE_HEIGHT = 5464;

        public const int CANON_PICTURE_EYE_SOURCE_WIDTH = 4096;
        public const int CANON_PICTURE_EYE_SOURCE_HEIGHT = 5464;

        public const int EQUIRECTANGULAR_TARGET_WIDTH = 4096;
        public const int EQUIRECTANGULAR_TARGET_HEIGHT = 4096;

        public const int FULL_TARGET_WIDTH = 8192;
        public const int FULL_TARGET_HEIGHT = 4096;


        //Assuming picture is split in two, sides already swapped, height set to 4096
        public const int RIGHT_CIRCLE_CENTER_X = 1955;
        public const int RIGHT_CIRCLE_CENTER_Y = 2037;
        public const int RIGHT_CIRCLE_RADIUS = 1845;

        public const int LEFT_CIRCLE_CENTER_X = 2130;
        public const int LEFT_CIRCLE_CENTER_Y = 2041;
        public const int LEFT_CIRCLE_RADIUS = 1849;
    }
}