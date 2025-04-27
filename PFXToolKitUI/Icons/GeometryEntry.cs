using PFXToolKitUI.Themes;

namespace PFXToolKitUI.Icons;

public readonly struct GeometryEntry {
    public readonly string Geometry;
    public readonly IColourBrush? Fill;
    public readonly IColourBrush? Stroke;
    public readonly double StrokeThickness;

    public GeometryEntry(string geometry, IColourBrush? fill, IColourBrush? stroke, double strokeThickness) {
        this.Geometry = geometry;
        this.Fill = fill;
        this.Stroke = stroke;
        this.StrokeThickness = strokeThickness;
    }

    public GeometryEntry(string geometry, IColourBrush? fill) {
        this.Geometry = geometry;
        this.Fill = fill;
    }

    public GeometryEntry(string geometry) {
        this.Geometry = geometry;
    }
}