namespace RiderProfit.Services.Ocr;

// One line of text read from an image, with its position on the image in pixels.
// Top and Left let the parser group lines that belong to the same trip card.
public record OcrLine(string Text, int Top, int Left);
