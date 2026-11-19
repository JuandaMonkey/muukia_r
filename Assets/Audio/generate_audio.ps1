# Script para sintetizar audio PCM WAV retro de alta calidad para MUUKIA
$audioDir = "Assets\MUUKIA\Audio"
if (!(Test-Path $audioDir)) { New-Item -ItemType Directory -Force -Path $audioDir }

function Write-WavFile($path, [float[]]$samples, $sampleRate = 22050) {
    $stream = [System.IO.File]::Create($path)
    $writer = New-Object System.IO.BinaryWriter($stream)

    $numSamples = $samples.Length
    $byteRate = $sampleRate * 2 # 16-bit mono
    $subChunk2Size = $numSamples * 2
    $chunkSize = 36 + $subChunk2Size

    # RIFF header
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes("RIFF"))
    $writer.Write([int32]$chunkSize)
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes("WAVE"))

    # fmt chunk
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes("fmt "))
    $writer.Write([int32]16) # SubChunk1Size (PCM)
    $writer.Write([int16]1)  # AudioFormat (PCM = 1)
    $writer.Write([int16]1)  # NumChannels (Mono = 1)
    $writer.Write([int32]$sampleRate)
    $writer.Write([int32]$byteRate)
    $writer.Write([int16]2)  # BlockAlign
    $writer.Write([int16]16) # BitsPerSample

    # data chunk
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes("data"))
    $writer.Write([int32]$subChunk2Size)

    foreach ($s in $samples) {
        $val = [Math]::Max(-1.0, [Math]::Min(1.0, $s))
        $shortVal = [int16]($val * 32767.0)
        $writer.Write($shortVal)
    }

    $writer.Close()
    $stream.Close()
}

$sampleRate = 22050
$rnd = New-Object System.Random(42)

# 1. Tenábaris (Rattling of Yaqui butterfly cocoon leg wraps)
# Multiple dry high-frequency noise bursts with sharp decay mimicking cocoon beads
$dur = 2.5
$n = [int]($dur * $sampleRate)
$tenabaris = New-Object float[] $n
$rattleRate = 14 # shakes per second
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $phase = ($t * $rattleRate) % 1.0
    $shakeEnv = [Math]::Pow([Math]::Sin($phase * [Math]::PI), 4.0) * [Math]::Exp(-$phase * 4.0)
    # Bandpass/resonant dry clicks
    $dryNoise = ($rnd.NextDouble() * 2.0 - 1.0)
    $cluster = [Math]::Sin(2.0 * [Math]::PI * 2800.0 * $t) * 0.3 + [Math]::Sin(2.0 * [Math]::PI * 4200.0 * $t) * 0.4
    $tenabaris[$i] = ($dryNoise * 0.6 + $cluster * 0.4) * $shakeEnv * 0.75
}
Write-WavFile "$audioDir\tenabaris_rattle.wav" $tenabaris $sampleRate

# 2. Tenábaris Loop (continuous sinister shaking)
$dur = 4.0
$n = [int]($dur * $sampleRate)
$tenabarisLoop = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $subEnv = 0.6 + 0.4 * [Math]::Sin(2.0 * [Math]::PI * 0.5 * $t)
    $phase = ($t * 12) % 1.0
    $shakeEnv = [Math]::Pow([Math]::Sin($phase * [Math]::PI), 3.0) * [Math]::Exp(-$phase * 3.5)
    $dryNoise = ($rnd.NextDouble() * 2.0 - 1.0)
    $tenabarisLoop[$i] = $dryNoise * $shakeEnv * $subEnv * 0.65
}
Write-WavFile "$audioDir\tenabaris_loop.wav" $tenabarisLoop $sampleRate

# 3. VHS Glitch & Tape Noise
$dur = 1.2
$n = [int]($dur * $sampleRate)
$vhsGlitch = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $white = ($rnd.NextDouble() * 2.0 - 1.0)
    $hum = [Math]::Sin(2.0 * [Math]::PI * 60.0 * $t) * 0.3 + [Math]::Sin(2.0 * [Math]::PI * 15734.0 * $t) * 0.1
    $burst = if ($i % 300 -lt 40) { ($rnd.NextDouble() * 2.0 - 1.0) * 0.8 } else { 0.0 }
    $vhsGlitch[$i] = ($white * 0.4 + $hum + $burst) * 0.7
}
Write-WavFile "$audioDir\vhs_glitch.wav" $vhsGlitch $sampleRate

# 4. VHS Static Hum (loopable background tape murmur)
$dur = 3.0
$n = [int]($dur * $sampleRate)
$vhsHum = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $white = ($rnd.NextDouble() * 2.0 - 1.0) * 0.15
    $lineHum = [Math]::Sin(2.0 * [Math]::PI * 50.0 * $t) * 0.2
    $vhsHum[$i] = $white + $lineHum
}
Write-WavFile "$audioDir\vhs_static_hum.wav" $vhsHum $sampleRate

# 5. Desert Wind Ambient (hollow, desolate Sonoran desert wind)
$dur = 6.0
$n = [int]($dur * $sampleRate)
$desertWind = New-Object float[] $n
$prev = 0.0
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $noise = ($rnd.NextDouble() * 2.0 - 1.0)
    $gust = 0.4 + 0.3 * [Math]::Sin(2.0 * [Math]::PI * 0.25 * $t) + 0.2 * [Math]::Sin(2.0 * [Math]::PI * 0.1 * $t)
    $prev = $prev * 0.94 + $noise * 0.06
    $desertWind[$i] = $prev * $gust * 1.5
}
Write-WavFile "$audioDir\desert_wind.wav" $desertWind $sampleRate

# 6. Crypts Ambient Drone (deep psychological horror drone)
$dur = 6.0
$n = [int]($dur * $sampleRate)
$cryptsDrone = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $drone1 = [Math]::Sin(2.0 * [Math]::PI * 45.0 * $t) * 0.4
    $drone2 = [Math]::Sin(2.0 * [Math]::PI * 48.0 * $t) * 0.35 # beat frequency dissonant
    $drone3 = [Math]::Sin(2.0 * [Math]::PI * 110.0 * $t + [Math]::Sin(2.0 * [Math]::PI * 0.3 * $t)) * 0.15
    $breath = ($rnd.NextDouble() * 2.0 - 1.0) * 0.04
    $cryptsDrone[$i] = $drone1 + $drone2 + $drone3 + $breath
}
Write-WavFile "$audioDir\crypts_drone.wav" $cryptsDrone $sampleRate

# 7. Urban Ambience (Obregon street tone with distant siren murmur)
$dur = 5.0
$n = [int]($dur * $sampleRate)
$urban = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $rumble = [Math]::Sin(2.0 * [Math]::PI * 80.0 * $t) * 0.15 + ($rnd.NextDouble() * 2.0 - 1.0) * 0.05
    $sirenFreq = 650.0 + 150.0 * [Math]::Sin(2.0 * [Math]::PI * 0.4 * $t)
    $distantSiren = [Math]::Sin(2.0 * [Math]::PI * $sirenFreq * $t) * 0.08
    $urban[$i] = $rumble + $distantSiren
}
Write-WavFile "$audioDir\urban_ambience.wav" $urban $sampleRate

# 8. Car engine loop, brake screech, and crash
$dur = 3.0
$n = [int]($dur * $sampleRate)
$engine = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $eng = [Math]::Sin(2.0 * [Math]::PI * 90.0 * $t) * 0.3 + [Math]::Sin(2.0 * [Math]::PI * 180.0 * $t) * 0.2 + ($rnd.NextDouble() * 2.0 - 1.0) * 0.1
    $engine[$i] = $eng * 0.7
}
Write-WavFile "$audioDir\car_engine.wav" $engine $sampleRate

# Brake screech
$dur = 1.8
$n = [int]($dur * $sampleRate)
$brake = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $screechFreq = 2200.0 - 600.0 * ($t / $dur)
    $sc = [Math]::Sin(2.0 * [Math]::PI * $screechFreq * $t) * 0.5 + ($rnd.NextDouble() * 2.0 - 1.0) * 0.3
    $env = [Math]::Sin(($t / $dur) * [Math]::PI)
    $brake[$i] = $sc * $env * 0.7
}
Write-WavFile "$audioDir\car_brake.wav" $brake $sampleRate

# Crash impact
$dur = 1.5
$n = [int]($dur * $sampleRate)
$crash = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $metalNoise = ($rnd.NextDouble() * 2.0 - 1.0) * [Math]::Exp(-$t * 6.0) * 0.8
    $thud = [Math]::Sin(2.0 * [Math]::PI * 55.0 * $t) * [Math]::Exp(-$t * 4.0) * 0.6
    $crash[$i] = $metalNoise + $thud
}
Write-WavFile "$audioDir\crash_impact.wav" $crash $sampleRate

# 9. Footsteps (dirt / gravel)
$dur = 0.25
$n = [int]($dur * $sampleRate)
$step = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $crunch = ($rnd.NextDouble() * 2.0 - 1.0) * [Math]::Exp(-$t * 22.0) * 0.5
    $step[$i] = $crunch
}
Write-WavFile "$audioDir\footstep_dirt.wav" $step $sampleRate

# 10. Dialogue Typewriter Blip
$dur = 0.05
$n = [int]($dur * $sampleRate)
$blip = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $blip[$i] = [Math]::Sin(2.0 * [Math]::PI * 950.0 * $t) * [Math]::Exp(-$t * 60.0) * 0.35
}
Write-WavFile "$audioDir\dialogue_blip.wav" $blip $sampleRate

# 11. Gustavo Agitated Breathing
$dur = 2.0
$n = [int]($dur * $sampleRate)
$breath = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $in = [Math]::Sin(($t / $dur) * 2.0 * [Math]::PI)
    $air = ($rnd.NextDouble() * 2.0 - 1.0) * 0.15 * [Math]::Max(0.0, $in)
    $breath[$i] = $air
}
Write-WavFile "$audioDir\gustavo_breath.wav" $breath $sampleRate

# 12. Ramírez Radio Voice Memo Beep & Transmission Tone
$dur = 1.0
$n = [int]($dur * $sampleRate)
$radioTone = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $b = if ($t -lt 0.15) { [Math]::Sin(2.0 * [Math]::PI * 880.0 * $t) * 0.3 } else { ($rnd.NextDouble() * 2.0 - 1.0) * 0.05 }
    $radioTone[$i] = $b
}
Write-WavFile "$audioDir\ramirez_voicenote.wav" $radioTone $sampleRate

# 13. Sofía Whisper Distant
$dur = 2.0
$n = [int]($dur * $sampleRate)
$whisper = New-Object float[] $n
for ($i = 0; $i -lt $n; $i++) {
    $t = $i / $sampleRate
    $formant1 = [Math]::Sin(2.0 * [Math]::PI * 320.0 * $t) * 0.1
    $formant2 = [Math]::Sin(2.0 * [Math]::PI * 1800.0 * $t) * 0.08
    $air = ($rnd.NextDouble() * 2.0 - 1.0) * 0.12
    $env = [Math]::Sin(($t / $dur) * [Math]::PI)
    $whisper[$i] = ($formant1 + $formant2 + $air) * $env * 0.5
}
Write-WavFile "$audioDir\sofia_whisper.wav" $whisper $sampleRate

Write-Output "All 13 audio clips generated successfully!"
