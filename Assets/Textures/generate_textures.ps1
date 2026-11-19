Add-Type -AssemblyName System.Drawing

$texDir = "Assets\MUUKIA\Textures"
if (!(Test-Path $texDir)) { New-Item -ItemType Directory -Force -Path $texDir }

function Create-PS2Texture($path, $width, $height, [ScriptBlock]$painter) {
    $bmp = New-Object System.Drawing.Bitmap($width, $height)
    $gfx = [System.Drawing.Graphics]::FromImage($bmp)
    $gfx.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $gfx.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    & $painter $gfx $bmp $width $height
    $gfx.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
}

# 1. Texture Gustavo (256x256)
Create-PS2Texture "$texDir\tex_gustavo.png" 256 256 {
    param($g, $bmp, $w, $h)
    # Background / Skin base: Mexican tan
    $skin = [System.Drawing.Color]::FromArgb(204, 153, 120)
    $skinShadow = [System.Drawing.Color]::FromArgb(160, 115, 90)
    $darkHair = [System.Drawing.Color]::FromArgb(35, 28, 25)
    $shirtKhaki = [System.Drawing.Color]::FromArgb(170, 150, 125)
    $shirtShadow = [System.Drawing.Color]::FromArgb(135, 118, 98)
    $pantsBrown = [System.Drawing.Color]::FromArgb(120, 100, 80)
    $beltLeather = [System.Drawing.Color]::FromArgb(75, 45, 30)
    $boots = [System.Drawing.Color]::FromArgb(40, 35, 30)

    # Face & Head region (0..128, 0..128)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skin)), 0, 0, 128, 128)
    # Hair
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($darkHair)), 0, 0, 128, 35)
    # Tired Eyes / Dark circles
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skinShadow)), 25, 48, 30, 18)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skinShadow)), 73, 48, 30, 18)
    # Eyes
    $g.FillRectangle([System.Drawing.Brushes]::White, 30, 52, 20, 8)
    $g.FillRectangle([System.Drawing.Brushes]::White, 78, 52, 20, 8)
    $g.FillRectangle([System.Drawing.Brushes]::Black, 38, 52, 8, 8)
    $g.FillRectangle([System.Drawing.Brushes]::Black, 86, 52, 8, 8)
    # Nose & Mouth
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skinShadow)), 58, 65, 12, 18)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skinShadow)), 50, 92, 28, 6)

    # Torso / Chamarra / Shirt (128..256, 0..128)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($shirtKhaki)), 128, 0, 128, 128)
    # Collar & pockets
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($shirtShadow)), 160, 0, 64, 25)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($shirtShadow)), 140, 45, 40, 35)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($shirtShadow)), 204, 45, 40, 35)
    # Belt
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($beltLeather)), 128, 110, 128, 18)
    $g.FillRectangle([System.Drawing.Brushes]::Silver, 180, 110, 24, 18)

    # Legs & Pants (0..128, 128..256)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($pantsBrown)), 0, 128, 128, 128)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($shirtShadow)), 55, 128, 18, 128)

    # Boots & Details (128..256, 128..256)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($boots)), 128, 128, 128, 128)
    $g.FillRectangle([System.Drawing.Brushes]::DimGray, 128, 220, 128, 36)
}

# 2. Texture Mother (256x256)
Create-PS2Texture "$texDir\tex_mother.png" 256 256 {
    param($g, $bmp, $w, $h)
    $skin = [System.Drawing.Color]::FromArgb(195, 145, 115)
    $skinShadow = [System.Drawing.Color]::FromArgb(150, 110, 85)
    $greyHair = [System.Drawing.Color]::FromArgb(85, 80, 80)
    $sweater = [System.Drawing.Color]::FromArgb(120, 95, 115) # muted mauve/plum cardigan
    $skirt = [System.Drawing.Color]::FromArgb(70, 75, 85)

    # Face
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skin)), 0, 0, 128, 128)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($greyHair)), 0, 0, 128, 40)
    # Kind, vulnerable, tired eyes
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skinShadow)), 28, 54, 25, 14)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skinShadow)), 75, 54, 25, 14)
    $g.FillRectangle([System.Drawing.Brushes]::White, 32, 57, 16, 7)
    $g.FillRectangle([System.Drawing.Brushes]::White, 79, 57, 16, 7)
    $g.FillRectangle([System.Drawing.Brushes]::Black, 38, 57, 6, 7)
    $g.FillRectangle([System.Drawing.Brushes]::Black, 85, 57, 6, 7)
    # Wrinkles / Sorrow
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skinShadow)), 30, 70, 18, 4)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skinShadow)), 80, 70, 18, 4)

    # Sweater
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($sweater)), 128, 0, 128, 128)
    $g.FillRectangle([System.Drawing.Brushes]::DarkSlateGray, 180, 0, 24, 128)

    # Skirt & Shoes
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skirt)), 0, 128, 256, 128)
}

# 3. Texture Neighbor (256x256)
Create-PS2Texture "$texDir\tex_neighbor.png" 256 256 {
    param($g, $bmp, $w, $h)
    $skin = [System.Drawing.Color]::FromArgb(185, 135, 105)
    $strawHat = [System.Drawing.Color]::FromArgb(215, 195, 140)
    $plaid1 = [System.Drawing.Color]::FromArgb(160, 180, 190)
    $plaid2 = [System.Drawing.Color]::FromArgb(120, 140, 155)

    # Head & Sombrero
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($skin)), 0, 0, 128, 128)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($strawHat)), 0, 0, 128, 45)
    # Eyes & Mustache
    $g.FillRectangle([System.Drawing.Brushes]::Black, 35, 58, 12, 6)
    $g.FillRectangle([System.Drawing.Brushes]::Black, 81, 58, 12, 6)
    $g.FillRectangle([System.Drawing.Brushes]::DarkGray, 45, 78, 38, 12)

    # Plaid Shirt
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($plaid1)), 128, 0, 128, 128)
    for ($i = 128; $i -lt 256; $i += 16) {
        $g.FillRectangle((New-Object System.Drawing.SolidBrush($plaid2)), $i, 0, 8, 128)
        $g.FillRectangle((New-Object System.Drawing.SolidBrush($plaid2)), 128, ($i - 128), 128, 8)
    }

    # Pants & Huaraches
    $g.FillRectangle([System.Drawing.Brushes]::DarkOliveGreen, 0, 128, 256, 128)
}

# 4. Texture MUUKIA Entity (256x256)
Create-PS2Texture "$texDir\tex_muukia.png" 256 256 {
    param($g, $bmp, $w, $h)
    $darkBody = [System.Drawing.Color]::FromArgb(15, 15, 20)
    $ribbonRed = [System.Drawing.Color]::FromArgb(185, 30, 30)
    $antlerBone = [System.Drawing.Color]::FromArgb(160, 150, 135)
    $tenabaris = [System.Drawing.Color]::FromArgb(210, 180, 130) # dried cocoon beige
    $whiteCloth = [System.Drawing.Color]::FromArgb(200, 200, 200)

    # Shadow Body
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($darkBody)), 0, 0, 256, 256)

    # Glowing Red Eyes region (intense emission red)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($ribbonRed)), 20, 20, 88, 50)
    $g.FillEllipse([System.Drawing.Brushes]::Red, 35, 30, 22, 22)
    $g.FillEllipse([System.Drawing.Brushes]::Red, 70, 30, 22, 22)
    $g.FillEllipse([System.Drawing.Brushes]::White, 42, 37, 8, 8)
    $g.FillEllipse([System.Drawing.Brushes]::White, 77, 37, 8, 8)

    # Deer Antlers / Headdress
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($antlerBone)), 128, 0, 128, 80)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($ribbonRed)), 128, 70, 128, 15)

    # Ceremonial White Cloth (Danzante waist wrap)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($whiteCloth)), 0, 128, 128, 64)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($darkBody)), 20, 150, 88, 10)

    # Tenábaris (Cocoon shakers wrapped around lower legs)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($tenabaris)), 128, 128, 128, 128)
    for ($y = 128; $y -lt 256; $y += 12) {
        for ($x = 128; $x -lt 256; $x += 12) {
            $g.FillEllipse([System.Drawing.Brushes]::SaddleBrown, $x + 2, $y + 2, 7, 7)
        }
    }
}

# 5. Environment Atlas (Sonoran Brick, Stucco, Concrete, Asphalt, Road Line, Crypt Stone)
Create-PS2Texture "$texDir\tex_environment.png" 256 256 {
    param($g, $bmp, $w, $h)
    # Sector 0,0: Sonoran Red Brick (Castillo & houses)
    $brickBase = [System.Drawing.Color]::FromArgb(165, 82, 54)
    $mortar = [System.Drawing.Color]::FromArgb(190, 175, 160)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($brickBase)), 0, 0, 128, 128)
    for ($y = 0; $y -lt 128; $y += 16) {
        $g.FillRectangle((New-Object System.Drawing.SolidBrush($mortar)), 0, $y, 128, 2)
        $offset = if (($y / 16) % 2 -eq 0) { 0 } else { 16 }
        for ($x = $offset; $x -lt 128; $x += 32) {
            $g.FillRectangle((New-Object System.Drawing.SolidBrush($mortar)), $x, $y, 2, 16)
        }
    }

    # Sector 128,0: Weathered Stucco / Mexican Concrete Plaster
    $stucco = [System.Drawing.Color]::FromArgb(215, 205, 185)
    $stuccoShadow = [System.Drawing.Color]::FromArgb(180, 170, 150)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($stucco)), 128, 0, 128, 128)
    $rnd = New-Object System.Random(101)
    for ($i = 0; $i -lt 120; $i++) {
        $g.FillRectangle((New-Object System.Drawing.SolidBrush($stuccoShadow)), 128 + $rnd.Next(124), $rnd.Next(124), 3, 3)
    }

    # Sector 0,128: Asphalt Road & Painted Yellow Dashes
    $asphalt = [System.Drawing.Color]::FromArgb(55, 55, 58)
    $yellow = [System.Drawing.Color]::FromArgb(220, 185, 45)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($asphalt)), 0, 128, 128, 128)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($yellow)), 56, 140, 16, 45)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($yellow)), 56, 210, 16, 45)

    # Sector 128,128: Crypt / Mausoleum Weathered Granite Stone
    $stone = [System.Drawing.Color]::FromArgb(110, 115, 120)
    $stoneDark = [System.Drawing.Color]::FromArgb(75, 80, 85)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($stone)), 128, 128, 128, 128)
    for ($y = 128; $y -lt 256; $y += 24) {
        $g.FillRectangle((New-Object System.Drawing.SolidBrush($stoneDark)), 128, $y, 128, 2)
    }
}

# 6. Props Texture (Pill bottles, camera, tape, Danzante monument stone)
Create-PS2Texture "$texDir\tex_props.png" 256 256 {
    param($g, $bmp, $w, $h)
    # Background
    $g.FillRectangle([System.Drawing.Brushes]::Gray, 0, 0, 256, 256)

    # Pill Bottles (Orange prescription bottle with white cap)
    $amber = [System.Drawing.Color]::FromArgb(220, 120, 20)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($amber)), 10, 10, 45, 65)
    $g.FillRectangle([System.Drawing.Brushes]::White, 12, 10, 41, 15) # Cap
    $g.FillRectangle([System.Drawing.Brushes]::White, 15, 35, 35, 30) # Label
    $g.DrawString("MED", (New-Object System.Drawing.Font("Arial", 8)), [System.Drawing.Brushes]::Black, 18, 40)

    # Crime Scene Police Tape (Yellow with black text)
    $g.FillRectangle([System.Drawing.Brushes]::Yellow, 0, 100, 256, 30)
    $font = New-Object System.Drawing.Font("Arial", 9, [System.Drawing.FontStyle]::Bold)
    $g.DrawString("PROHIBIDO EL PASO - POLICIA - NO CRUZAR", $font, [System.Drawing.Brushes]::Black, 5, 106)

    # Video Camcorder Prop
    $g.FillRectangle([System.Drawing.Brushes]::Black, 100, 10, 70, 50)
    $g.FillEllipse([System.Drawing.Brushes]::DarkSlateBlue, 150, 20, 20, 20) # Lens
    $g.FillRectangle([System.Drawing.Brushes]::Red, 110, 20, 8, 8) # Rec LED

    # Danzante Monument Weathered Bronze/Stone
    $bronze = [System.Drawing.Color]::FromArgb(85, 95, 75)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($bronze)), 0, 150, 256, 106)
}

Write-Output "Textures generated successfully in PS2 aesthetic!"
