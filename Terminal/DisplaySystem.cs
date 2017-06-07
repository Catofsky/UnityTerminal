using UnityEngine;

public class DisplaySystem {

    public const int CHAR_WIDTH = 4;
    public const int CHAR_HEIGHT = 8;
    public const int COLS = 36;
    public const int LINES = 18;
    public const int WIDTH = COLS * CHAR_WIDTH;
    public const int HEIGHT = LINES * CHAR_HEIGHT;

    private Texture2D atlas;
    private Texture2D texture;
    private Texture2D[] frames;

    private Color fontColor = new Color(1, 1, 1);
    private Color backgroundColor = new Color(0.05f, 0.05f, 0.6f);
    private Color currentFontColor;
    private Color currentBackColor;

    private int[,] textBuffer = new int[COLS, LINES];
    private Color[,] textColorBuffer = new Color[COLS, LINES];
    private Color[,] backColorBuffer = new Color[COLS, LINES];

    public DisplaySystem(MeshRenderer mesh) {
        texture = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Point;
        atlas = Resources.Load("font") as Texture2D;
        frames = Split2D(atlas, CHAR_HEIGHT * 2, CHAR_WIDTH * 2);
        currentFontColor = fontColor;
        currentBackColor = backgroundColor;
        Clear();
        texture.Apply(false);

        mesh.materials[0].SetTexture("_MainTex", texture);
        mesh.materials[0].SetTexture("_EmissionMap", texture);
    }

    private Texture2D[] Split2D(Texture2D texture, int x, int y) {
        int sprX = texture.width / x;
        int sprY = texture.height / y;
        Texture2D[] res = new Texture2D[x * y];
        for (int j = 0; j < y; j++)
            for (int i = 0; i < x; i++) {
                res[j * x + i] = new Texture2D(sprX, sprY);
                res[j * x + i].SetPixels(0, 0, sprX, sprY, texture.GetPixels(i * sprX, j * sprY, sprX, sprY));
            }
        return res;
    }

    private void SetPixels(int c, int x, int y, bool invert) {
        Color[] pixels = new Color[frames[0].height * frames[0].width];
        pixels = frames[c].GetPixels();
        if (invert) {
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i].Equals(Color.black))
                    pixels[i] = currentFontColor;
                else
                    pixels[i] = currentBackColor;
        }
        else {
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i].Equals(Color.black))
                    pixels[i] = currentBackColor;
                else
                    pixels[i] = currentFontColor;
        }
        texture.SetPixels(x * CHAR_WIDTH, HEIGHT - y * CHAR_HEIGHT - CHAR_HEIGHT, CHAR_WIDTH, CHAR_HEIGHT, pixels);
    }

    private void SetPixels(int c, int x, int y, bool invert, Color font, Color back) {
        Color[] pixels = new Color[frames[0].height * frames[0].width];
        pixels = frames[c].GetPixels();
        if (invert) {
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i].Equals(Color.black))
                    pixels[i] = font;
                else
                    pixels[i] = back;
        }
        else {
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i].Equals(Color.black))
                    pixels[i] = back;
                else
                    pixels[i] = font;
        }
        texture.SetPixels(x * CHAR_WIDTH, HEIGHT - y * CHAR_HEIGHT - CHAR_HEIGHT, CHAR_WIDTH, CHAR_HEIGHT, pixels);
    }

    public void TypeChar(int c, int x, int y) {
        if (x > COLS - 1 || x < 0 || y > LINES - 1 || y < 0) return;
        SetPixels(c, x, y, false);
        textBuffer[x, y] = c;
        textColorBuffer[x, y] = currentFontColor;
        backColorBuffer[x, y] = currentBackColor;
    }

    public void TypeChar(int c, int x, int y, Color font, Color back) {
        if (x > COLS - 1 || x < 0 || y > LINES - 1 || y < 0) return;
        SetPixels(c, x, y, false, font, back);
        textBuffer[x, y] = c;
        textColorBuffer[x, y] = font;
        backColorBuffer[x, y] = back;
    }

    public void SetInvertBlock(int x, int y) {
        if (x > COLS - 1 || x < 0 || y > LINES - 1 || y < 0) return;
        SetPixels(textBuffer[x, y], x, y, true);
    }

    public void ResetInvertBlock(int x, int y) {
        if (x > COLS - 1 || x < 0 || y > LINES - 1 || y < 0) return;
        SetPixels(textBuffer[x, y], x, y, false);
    }

    public void DrawPixel(int x, int y, Color c) {
        if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT) return;
        texture.SetPixel(x, HEIGHT - y, c);
    }

    public void DrawImage(int x, int y, int sizeX, int sizeY, Color[] c) {
        if (x < 0 || y < 0 || x >= WIDTH + sizeX || y >= HEIGHT + sizeY) return;
        if (sizeX * sizeY != c.Length) return;
        texture.SetPixels(x, HEIGHT - y - 1, sizeX, sizeY, c);
    }

    public void DrawPixels(int x, int y, int sizeX, int sizeY, Color c) {
        if (x < 0 || y < 0 || x + sizeX >= WIDTH || -y >= 1) return;
        Color[] col = new Color[sizeX * sizeY];
        for (int i = 0; i < col.Length; i++) {
            col[i] = c;
        }
        texture.SetPixels(x, HEIGHT - y - 1 - sizeY, sizeX, sizeY, col);
    }

    public void DrawLine(int x0, int y0, int x1, int y1, Color c) {
        if (x0 < 0 || y0 < 0 || x1 < 0 || y1 < 0 || x0 >= WIDTH || y0 >= HEIGHT || x1 >= WIDTH || y1 >= HEIGHT) return;
        y0 = HEIGHT - y0 - 1;
        y1 = HEIGHT - y1 - 1;
        int dy = y1 - y0;
        int dx = x1 - x0;
        int stepx, stepy;
        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;
        float fraction = 0;
        texture.SetPixel(x0, y0, c);
        if (dx > dy) {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1) {
                if (fraction >= 0) {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;
                texture.SetPixel(x0, y0, c);
            }
        }
        else {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1) {
                if (fraction >= 0) {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;
                texture.SetPixel(x0, y0, c);
            }
        }
    }

    public void DrawCircle(int x, int y, int r, Color c) {
        if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT) return;
        int j = r;
        y = HEIGHT - y;
        int d = 1 / 4 - r;
        float end = Mathf.Ceil(r / Mathf.Sqrt(2));
        for (int i = 0; i <= end; i++) {
            texture.SetPixel(x + i, y + j, c);
            texture.SetPixel(x + i, y - j, c);
            texture.SetPixel(x - i, y + j, c);
            texture.SetPixel(x - i, y - j, c);
            texture.SetPixel(x + j, y + i, c);
            texture.SetPixel(x - j, y + i, c);
            texture.SetPixel(x + j, y - i, c);
            texture.SetPixel(x - j, y - i, c);
            d += 2 * i + 1;
            if (d > 0) {
                d += 2 - 2 * j--;
            }
        }
    }

    public void DrawRectangle(int x, int y, int sizeX, int sizeY, Color c) {
        sizeX = sizeX - 1;
        sizeY = sizeY - 1;
        DrawLine(x, y, x + sizeX + 1, y, c);
        DrawLine(x, y, x, y + sizeY + 1, c);
        DrawLine(x + sizeX, y + sizeY, x + sizeX, y, c);
        DrawLine(x + sizeX, y + sizeY, x, y + sizeY, c);
    }

    public void Apply() {
        texture.Apply(false);
    }

    public void BlackScreen() {
        Color[] colors = new Color[WIDTH * HEIGHT];
        for (int i = 0; i < WIDTH * HEIGHT; i++) {
            colors[i] = Color.black;
        }
        texture.SetPixels(colors, 0);
    }

    public void Clear() {
        Color[] colors = new Color[WIDTH * HEIGHT];
        for (int i = 0; i < WIDTH * HEIGHT; i++) {
            colors[i] = currentBackColor;
        }
        texture.SetPixels(colors, 0);
        for (int i = 0; i < COLS; i++)
            for (int j = 0; j < LINES; j++) {
                textBuffer[i, j] = 0;
                textColorBuffer[i, j] = currentFontColor;
                backColorBuffer[i, j] = currentBackColor;
            }
    }

    public void ClearLine(int line) {
        for (int i = 0; i < COLS; i++) {
            TypeChar(0, i, line);
        }
    }

    public void ClearLine(int line, int start, int end) {
        for (int i = start; i < COLS - end; i++) {
            TypeChar(0, i, line);
        }
    }

    public void SetFontColor(Color c) {
        currentFontColor = c;
    }

    public void SetBackColor(Color c) {
        currentBackColor = c;
    }

    public Color GetFontColor() {
        return currentFontColor;
    }

    public Color GetBackColor() {
        return currentBackColor;
    }

    public void ResetColors() {
        currentFontColor = fontColor;
        currentBackColor = backgroundColor;
    }

    public int GetLineLength(int line) {
        int count = COLS - 1;
        while (count > 0 && (textBuffer[count, line] == 0 || textBuffer[count, line] == 1)) {
            count--;
        }
        return ++count;
    }

    public void ScrollUp() {
        Color tmp = currentFontColor;
        for (int j = 0; j < LINES; j++)
            for (int i = 0; i < COLS; i++) {
                if (j == LINES - 1) {
                    textBuffer[i, j] = 0;
                    textColorBuffer[i, j] = currentFontColor;
                    backColorBuffer[i, j] = currentBackColor;
                }
                else {
                    textBuffer[i, j] = textBuffer[i, j + 1];
                    textColorBuffer[i, j] = textColorBuffer[i, j + 1];
                    backColorBuffer[i, j] = backColorBuffer[i, j + 1];
                }
                currentFontColor = textColorBuffer[i, j];
                TypeChar(textBuffer[i, j], i, j);
            }
        currentFontColor = tmp;
    }

    public void ScrollUp(int width, int height) {
        Color tmp = currentFontColor;
        for (int j = height; j < LINES - height; j++)
            for (int i = width; i < COLS - width; i++) {
                if (j == LINES - 1 - height) {
                    textBuffer[i, j] = 0;
                    textColorBuffer[i, j] = currentFontColor;
                    backColorBuffer[i, j] = currentBackColor;
                }
                else {
                    textBuffer[i, j] = textBuffer[i, j + 1];
                    textColorBuffer[i, j] = textColorBuffer[i, j + 1];
                    backColorBuffer[i, j] = backColorBuffer[i, j + 1];
                }
                currentFontColor = textColorBuffer[i, j];
                TypeChar(textBuffer[i, j], i, j);
            }
        currentFontColor = tmp;
    }

    public void ScrollUp(int width, int height, int count) {
        Color tmp = currentFontColor;
        for (int j = height; j < LINES - height; j++)
            for (int i = width; i < COLS - width; i++) {
                if (j >= LINES - count - height) {
                    textBuffer[i, j] = 0;
                    textColorBuffer[i, j] = currentFontColor;
                    backColorBuffer[i, j] = currentBackColor;
                }
                else {
                    textBuffer[i, j] = textBuffer[i, j + count];
                    textColorBuffer[i, j] = textColorBuffer[i, j + count];
                    backColorBuffer[i, j] = backColorBuffer[i, j + count];
                }
                currentFontColor = textColorBuffer[i, j];
                TypeChar(textBuffer[i, j], i, j);
            }
        currentFontColor = tmp;
    }

}