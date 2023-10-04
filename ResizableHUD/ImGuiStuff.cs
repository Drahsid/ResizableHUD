using System.Numerics;

namespace ResizableHUD; 

internal class ImGuiStuff {
    public static unsafe uint ColorToUint(Vector4* color) {
        byte r = (byte)(color->X * 255);
        byte g = (byte)(color->Y * 255);
        byte b = (byte)(color->Z * 255);
        byte a = (byte)(color->W * 255);

        return (uint)((a << 24) | (r << 16) | (g << 8) | b);
    }

}
