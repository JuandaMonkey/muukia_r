$modelsChar = "Assets\MUUKIA\Models\Characters"
$modelsEnv  = "Assets\MUUKIA\Models\Environments"
$modelsProp = "Assets\MUUKIA\Models\Props"

class ObjBuilder {
    [System.Collections.ArrayList]$verts = [System.Collections.ArrayList]::new()
    [System.Collections.ArrayList]$uvs   = [System.Collections.ArrayList]::new()
    [System.Collections.ArrayList]$norms = [System.Collections.ArrayList]::new()
    [System.Collections.ArrayList]$faces = [System.Collections.ArrayList]::new()

    [int] AddVertex([float]$x, [float]$y, [float]$z) {
        return $this.verts.Add([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "v {0:F4} {1:F4} {2:F4}", $x, $y, $z)) + 1
    }

    [int] AddUV([float]$u, [float]$v) {
        return $this.uvs.Add([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "vt {0:F4} {1:F4}", $u, $v)) + 1
    }

    [int] AddNormal([float]$nx, [float]$ny, [float]$nz) {
        return $this.norms.Add([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "vn {0:F4} {1:F4} {2:F4}", $nx, $ny, $nz)) + 1
    }

    AddQuad([int]$v1, [int]$v2, [int]$v3, [int]$v4, [int]$uv1, [int]$uv2, [int]$uv3, [int]$uv4, [int]$n) {
        # Triangle 1: v1, v2, v3
        $this.faces.Add("f {0}/{1}/{6} {2}/{3}/{6} {4}/{5}/{6}" -f @($v1, $uv1, $v2, $uv2, $v3, $uv3, $n)) | Out-Null
        # Triangle 2: v1, v3, v4
        $this.faces.Add("f {0}/{1}/{6} {2}/{3}/{6} {4}/{5}/{6}" -f @($v1, $uv1, $v3, $uv3, $v4, $uv4, $n)) | Out-Null
    }

    AddBox([float]$cx, [float]$cy, [float]$cz, [float]$sx, [float]$sy, [float]$sz, [float]$uMin=0.0, [float]$vMin=0.0, [float]$uMax=1.0, [float]$vMax=1.0) {
        $hx = $sx * 0.5; $hy = $sy * 0.5; $hz = $sz * 0.5
        $nFront = $this.AddNormal(0, 0, 1)
        $nBack  = $this.AddNormal(0, 0, -1)
        $nTop   = $this.AddNormal(0, 1, 0)
        $nBot   = $this.AddNormal(0, -1, 0)
        $nRight = $this.AddNormal(1, 0, 0)
        $nLeft  = $this.AddNormal(-1, 0, 0)

        $uv1 = $this.AddUV($uMin, $vMin)
        $uv2 = $this.AddUV($uMax, $vMin)
        $uv3 = $this.AddUV($uMax, $vMax)
        $uv4 = $this.AddUV($uMin, $vMax)

        $v1 = $this.AddVertex($cx - $hx, $cy - $hy, $cz + $hz)
        $v2 = $this.AddVertex($cx + $hx, $cy - $hy, $cz + $hz)
        $v3 = $this.AddVertex($cx + $hx, $cy + $hy, $cz + $hz)
        $v4 = $this.AddVertex($cx - $hx, $cy + $hy, $cz + $hz)
        $v5 = $this.AddVertex($cx - $hx, $cy - $hy, $cz - $hz)
        $v6 = $this.AddVertex($cx + $hx, $cy - $hy, $cz - $hz)
        $v7 = $this.AddVertex($cx + $hx, $cy + $hy, $cz - $hz)
        $v8 = $this.AddVertex($cx - $hx, $cy + $hy, $cz - $hz)

        $this.AddQuad($v1, $v2, $v3, $v4, $uv1, $uv2, $uv3, $uv4, $nFront)
        $this.AddQuad($v6, $v5, $v8, $v7, $uv1, $uv2, $uv3, $uv4, $nBack)
        $this.AddQuad($v4, $v3, $v7, $v8, $uv1, $uv2, $uv3, $uv4, $nTop)
        $this.AddQuad($v5, $v6, $v2, $v1, $uv1, $uv2, $uv3, $uv4, $nBot)
        $this.AddQuad($v2, $v6, $v7, $v3, $uv1, $uv2, $uv3, $uv4, $nRight)
        $this.AddQuad($v5, $v1, $v4, $v8, $uv1, $uv2, $uv3, $uv4, $nLeft)
    }

    Save([string]$filePath) {
        $sb = [System.Text.StringBuilder]::new()
        $sb.AppendLine("# MUUKIA PS2 Low-Poly Model") | Out-Null
        foreach ($v in $this.verts) { $sb.AppendLine($v) | Out-Null }
        foreach ($vt in $this.uvs) { $sb.AppendLine($vt) | Out-Null }
        foreach ($vn in $this.norms) { $sb.AppendLine($vn) | Out-Null }
        foreach ($f in $this.faces) { $sb.AppendLine($f) | Out-Null }
        [System.IO.File]::WriteAllText($filePath, $sb.ToString())
    }
}

# 1. Gustavo Model (low-poly humanoid reporter PS2)
$b = [ObjBuilder]::new()
$b.AddBox(0, 1.62, 0, 0.22, 0.24, 0.22, 0.0, 0.5, 0.5, 1.0) # Head
$b.AddBox(0, 1.60, -0.14, 0.08, 0.12, 0.08, 0.0, 0.8, 0.2, 1.0) # Ponytail
$b.AddBox(0, 1.48, 0, 0.12, 0.08, 0.12, 0.0, 0.5, 0.2, 0.7) # Neck
$b.AddBox(0, 1.18, 0, 0.42, 0.54, 0.26, 0.5, 0.5, 1.0, 1.0) # Torso
$b.AddBox(-0.28, 1.16, 0, 0.14, 0.50, 0.14, 0.5, 0.5, 0.8, 0.8) # Left Arm
$b.AddBox(0.28, 1.16, 0.08, 0.14, 0.48, 0.14, 0.5, 0.5, 0.8, 0.8) # Right Arm
$b.AddBox(0.28, 1.05, 0.22, 0.12, 0.10, 0.18, 0.4, 0.8, 0.6, 1.0) # Camera
$b.AddBox(0, 0.86, 0, 0.38, 0.14, 0.24, 0.5, 0.4, 1.0, 0.5) # Belt
$b.AddBox(-0.12, 0.46, 0, 0.16, 0.68, 0.18, 0.0, 0.0, 0.5, 0.5) # Left Leg
$b.AddBox(0.12, 0.46, 0, 0.16, 0.68, 0.18, 0.0, 0.0, 0.5, 0.5)  # Right Leg
$b.AddBox(-0.12, 0.06, 0.04, 0.18, 0.14, 0.26, 0.5, 0.0, 1.0, 0.5) # Left Boot
$b.AddBox(0.12, 0.06, 0.04, 0.18, 0.14, 0.26, 0.5, 0.0, 1.0, 0.5)  # Right Boot
$b.Save("$modelsChar\Gustavo.obj")

# 2. Mother Model
$b = [ObjBuilder]::new()
$b.AddBox(0, 1.45, 0, 0.20, 0.22, 0.20, 0.0, 0.5, 0.5, 1.0)
$b.AddBox(0, 1.48, -0.12, 0.10, 0.10, 0.08, 0.0, 0.8, 0.3, 1.0)
$b.AddBox(0, 1.08, 0, 0.38, 0.50, 0.26, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(-0.24, 1.06, 0.04, 0.12, 0.44, 0.12, 0.5, 0.5, 0.8, 0.8)
$b.AddBox(0.24, 1.06, 0.04, 0.12, 0.44, 0.12, 0.5, 0.5, 0.8, 0.8)
$b.AddBox(0, 0.48, 0, 0.40, 0.72, 0.30, 0.0, 0.0, 1.0, 0.5)
$b.AddBox(-0.10, 0.05, 0.02, 0.14, 0.10, 0.20, 0.6, 0.0, 0.9, 0.3)
$b.AddBox(0.10, 0.05, 0.02, 0.14, 0.10, 0.20, 0.6, 0.0, 0.9, 0.3)
$b.Save("$modelsChar\Mother.obj")

# 3. Neighbor Model
$b = [ObjBuilder]::new()
$b.AddBox(0, 0.42, 0, 0.55, 0.06, 0.50, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(0, 0.80, -0.22, 0.55, 0.70, 0.06, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(-0.24, 0.20, -0.20, 0.06, 0.40, 0.06, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(0.24, 0.20, -0.20, 0.06, 0.40, 0.06, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(-0.24, 0.20, 0.20, 0.06, 0.40, 0.06, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(0.24, 0.20, 0.20, 0.06, 0.40, 0.06, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(0, 0.82, -0.06, 0.40, 0.52, 0.26, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 1.25, -0.06, 0.22, 0.22, 0.22, 0.0, 0.5, 0.5, 1.0)
$b.AddBox(0, 1.34, -0.06, 0.48, 0.06, 0.48, 0.0, 0.7, 0.5, 1.0)
$b.AddBox(-0.22, 0.75, 0.10, 0.12, 0.38, 0.24, 0.5, 0.5, 0.9, 0.9)
$b.AddBox(0.22, 0.75, 0.10, 0.12, 0.38, 0.24, 0.5, 0.5, 0.9, 0.9)
$b.AddBox(-0.12, 0.48, 0.18, 0.16, 0.16, 0.44, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0.12, 0.48, 0.18, 0.16, 0.16, 0.44, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(-0.12, 0.22, 0.36, 0.16, 0.40, 0.16, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0.12, 0.22, 0.36, 0.16, 0.40, 0.16, 0.0, 0.0, 0.5, 0.5)
$b.Save("$modelsChar\Neighbor.obj")

# 4. MUUKIA Entity Model
$b = [ObjBuilder]::new()
$b.AddBox(0, 1.55, 0, 0.36, 0.75, 0.22, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0, 1.15, 0, 0.40, 0.26, 0.26, 0.0, 0.5, 0.5, 0.8)
$b.AddBox(0, 2.02, 0, 0.12, 0.22, 0.12, 0.0, 0.0, 0.3, 0.3)
$b.AddBox(0, 2.22, 0, 0.22, 0.24, 0.22, 0.0, 0.0, 0.5, 0.3)
$b.AddBox(0, 2.38, 0.08, 0.18, 0.16, 0.32, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(-0.16, 2.60, 0.02, 0.06, 0.35, 0.06, 0.5, 0.6, 0.8, 1.0)
$b.AddBox(0.16, 2.60, 0.02, 0.06, 0.35, 0.06, 0.5, 0.6, 0.8, 1.0)
$b.AddBox(-0.28, 1.35, 0, 0.10, 0.95, 0.10, 0.0, 0.0, 0.4, 0.4)
$b.AddBox(0.28, 1.35, 0, 0.10, 0.95, 0.10, 0.0, 0.0, 0.4, 0.4)
$b.AddBox(-0.13, 0.58, 0, 0.12, 1.05, 0.12, 0.0, 0.0, 0.4, 0.4)
$b.AddBox(0.13, 0.58, 0, 0.12, 1.05, 0.12, 0.0, 0.0, 0.4, 0.4)
$b.AddBox(-0.13, 0.32, 0, 0.22, 0.40, 0.22, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0.13, 0.32, 0, 0.22, 0.40, 0.22, 0.5, 0.5, 1.0, 1.0)
$b.Save("$modelsChar\MuukiaEntity.obj")

# 5. Police Officer Model
$b = [ObjBuilder]::new()
$b.AddBox(0, 1.62, 0, 0.22, 0.22, 0.22, 0.0, 0.5, 0.5, 1.0)
$b.AddBox(0, 1.72, 0.04, 0.26, 0.08, 0.30, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0, 1.18, 0, 0.44, 0.54, 0.28, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(-0.28, 1.15, 0, 0.14, 0.48, 0.14, 0.5, 0.5, 0.8, 0.8)
$b.AddBox(0.28, 1.15, 0, 0.14, 0.48, 0.14, 0.5, 0.5, 0.8, 0.8)
$b.AddBox(0, 0.86, 0, 0.40, 0.14, 0.26, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(-0.13, 0.45, 0, 0.18, 0.70, 0.20, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0.13, 0.45, 0, 0.18, 0.70, 0.20, 0.0, 0.0, 0.5, 0.5)
$b.Save("$modelsChar\PoliceOfficer.obj")

# 6. Car Sedan (Gustavo's car)
$b = [ObjBuilder]::new()
$b.AddBox(0, 0.55, 0, 1.80, 0.48, 4.20, 0.0, 0.5, 0.5, 1.0)
$b.AddBox(0, 1.05, -0.20, 1.55, 0.52, 2.10, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 0.65, 1.30, 1.70, 0.15, 1.40, 0.0, 0.5, 0.5, 1.0)
$b.AddBox(0, 0.65, -1.55, 1.70, 0.15, 0.90, 0.0, 0.5, 0.5, 1.0)
$b.AddBox(-0.92, 0.32, 1.20, 0.24, 0.64, 0.64, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0.92, 0.32, 1.20, 0.24, 0.64, 0.64, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(-0.92, 0.32, -1.20, 0.24, 0.64, 0.64, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0.92, 0.32, -1.20, 0.24, 0.64, 0.64, 0.0, 0.0, 0.5, 0.5)
$b.Save("$modelsProp\CarSedan.obj")

# 7. Police Patrol Car
$b = [ObjBuilder]::new()
$b.AddBox(0, 0.55, 0, 1.85, 0.50, 4.30, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(0, 1.08, -0.20, 1.60, 0.54, 2.15, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(0, 1.42, -0.20, 1.10, 0.14, 0.26, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(-0.94, 0.32, 1.20, 0.24, 0.64, 0.64, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0.94, 0.32, 1.20, 0.24, 0.64, 0.64, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(-0.94, 0.32, -1.20, 0.24, 0.64, 0.64, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0.94, 0.32, -1.20, 0.24, 0.64, 0.64, 0.0, 0.0, 0.5, 0.5)
$b.Save("$modelsProp\PolicePatrol.obj")

# 8. Danzante Yaqui Monument
$b = [ObjBuilder]::new()
$b.AddBox(0, 1.5, 0, 4.0, 3.0, 4.0, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 3.2, 0, 3.5, 0.4, 3.5, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 5.0, 0, 1.4, 3.2, 1.0, 0.0, 0.6, 1.0, 1.0)
$b.AddBox(0, 7.2, 0, 0.9, 1.0, 0.9, 0.0, 0.6, 1.0, 1.0)
$b.AddBox(0, 8.2, 0.2, 0.8, 1.0, 1.4, 0.0, 0.6, 1.0, 1.0)
$b.AddBox(-0.7, 8.9, 0, 0.2, 0.9, 0.2, 0.0, 0.6, 1.0, 1.0)
$b.AddBox(0.7, 8.9, 0, 0.2, 0.9, 0.2, 0.0, 0.6, 1.0, 1.0)
$b.AddBox(-1.4, 6.4, 0.4, 0.4, 1.8, 0.4, 0.0, 0.6, 1.0, 1.0)
$b.AddBox(1.4, 6.4, 0.4, 0.4, 1.8, 0.4, 0.0, 0.6, 1.0, 1.0)
$b.Save("$modelsProp\DanzanteMonument.obj")

# 9. El Castillo de Dr. Miguel Maderos
$b = [ObjBuilder]::new()
$b.AddBox(0, 6.0, 0, 10.0, 12.0, 8.0, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(-6.0, 8.5, 0, 4.0, 17.0, 4.0, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(6.0, 7.5, 0, 4.0, 15.0, 4.0, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0, 2.5, 4.5, 4.5, 5.0, 2.0, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(0, 8.5, 4.05, 2.0, 3.0, 0.2, 0.0, 0.0, 0.2, 0.2)
$b.AddBox(-6.0, 12.0, 2.05, 1.4, 2.5, 0.2, 0.0, 0.0, 0.2, 0.2)
$b.AddBox(6.0, 11.0, 2.05, 1.4, 2.5, 0.2, 0.0, 0.0, 0.2, 0.2)
$b.AddBox(0, 12.4, 0, 10.4, 0.8, 8.4, 0.5, 0.0, 1.0, 0.5)
$b.Save("$modelsEnv\CastilloMaderos.obj")

# 10. Mausoleum
$b = [ObjBuilder]::new()
$b.AddBox(0, 2.5, 0, 5.0, 5.0, 6.0, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(-2.0, 2.2, 3.2, 0.5, 4.4, 0.5, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(2.0, 2.2, 3.2, 0.5, 4.4, 0.5, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 5.4, 0.2, 5.4, 0.8, 6.6, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 6.0, 0.2, 4.2, 0.6, 6.2, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 1.8, 3.05, 1.8, 3.4, 0.2, 0.0, 0.0, 0.2, 0.2)
$b.Save("$modelsEnv\Mausoleum.obj")

# 11. Obelisk
$b = [ObjBuilder]::new()
$b.AddBox(0, 0.6, 0, 2.0, 1.2, 2.0, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 2.2, 0, 1.5, 2.0, 1.5, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 5.0, 0, 1.0, 4.0, 1.0, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 7.3, 0, 0.6, 1.0, 0.6, 0.5, 0.5, 1.0, 1.0)
$b.Save("$modelsEnv\Obelisk.obj")

# 12. Collapsed Crypt
$b = [ObjBuilder]::new()
$b.AddBox(-2.0, 1.8, 0, 1.0, 3.6, 5.0, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(2.0, 1.8, 0, 1.0, 3.6, 5.0, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, 1.8, -2.5, 4.0, 3.6, 0.8, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(-1.6, 1.8, 2.5, 1.4, 3.6, 0.8, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(1.6, 1.8, 2.5, 1.4, 3.6, 0.8, 0.5, 0.5, 1.0, 1.0)
$b.AddBox(0, -1.0, 0, 2.5, 2.0, 4.0, 0.0, 0.0, 0.1, 0.1)
$b.Save("$modelsEnv\CollapsedCrypt.obj")

# 13. Mexican House
$b = [ObjBuilder]::new()
$b.AddBox(0, 2.2, 0, 8.0, 4.4, 9.0, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(0, 1.2, 4.55, 1.4, 2.4, 0.1, 0.0, 0.0, 0.2, 0.2)
$b.AddBox(-2.5, 2.0, 4.55, 1.6, 1.4, 0.1, 0.0, 0.0, 0.2, 0.2)
$b.AddBox(2.5, 2.0, 4.55, 1.6, 1.4, 0.1, 0.0, 0.0, 0.2, 0.2)
$b.AddBox(0, 4.5, 0, 8.2, 0.4, 9.2, 0.5, 0.0, 1.0, 0.5)
$b.Save("$modelsEnv\MexicanHouse.obj")

# 14. Clinic Facade
$b = [ObjBuilder]::new()
$b.AddBox(0, 3.0, 0, 12.0, 6.0, 6.0, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(0, 3.8, 3.05, 6.0, 1.2, 0.2, 0.0, 0.5, 0.5, 1.0)
$b.AddBox(0, 1.5, 3.05, 3.0, 3.0, 0.2, 0.0, 0.0, 0.2, 0.2)
$b.Save("$modelsEnv\ClinicFacade.obj")

# 15. Gas Station
$b = [ObjBuilder]::new()
$b.AddBox(0, 4.5, 0, 10.0, 0.8, 7.0, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(-3.5, 2.2, 0, 0.6, 4.4, 0.6, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(3.5, 2.2, 0, 0.6, 4.4, 0.6, 0.5, 0.0, 1.0, 0.5)
$b.AddBox(-1.8, 0.8, 0, 0.8, 1.6, 0.6, 0.0, 0.5, 0.5, 1.0)
$b.AddBox(1.8, 0.8, 0, 0.8, 1.6, 0.6, 0.0, 0.5, 0.5, 1.0)
$b.Save("$modelsEnv\GasStation.obj")

# 16. Props
$b = [ObjBuilder]::new()
$b.AddBox(0, 0.06, 0, 0.08, 0.12, 0.08, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0.12, 0.05, 0, 0.07, 0.10, 0.07, 0.0, 0.0, 0.5, 0.5)
$b.Save("$modelsProp\PillsMedicine.obj")

$b = [ObjBuilder]::new()
$b.AddBox(-2.0, 0.6, 0, 0.1, 1.2, 0.1, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(2.0, 0.6, 0, 0.1, 1.2, 0.1, 0.0, 0.0, 0.5, 0.5)
$b.AddBox(0, 0.8, 0, 4.0, 0.18, 0.02, 0.0, 0.4, 1.0, 0.6)
$b.Save("$modelsProp\PoliceTape.obj")

Write-Output "SUCCESS: All 16 3D models generated cleanly!"
