from PIL import Image
import os

# Input and output directories
input_dir = r"E:\Github\Wrath_Auto_Tracker\SamplePlugin\SamplePlugin\bin\x64\Debug\Resources"
output_dir = r"E:\Github\Wrath_Auto_Tracker\SamplePlugin\SamplePlugin\bin\x64\Debug\Resources\Processed"

# Ensure the output directory exists
os.makedirs(output_dir, exist_ok=True)

# Desired image size (optional, set None if resizing is not needed)
desired_size = (64, 64)  # Example: Resize to 64x64 pixels

# Process each PNG image in the input directory
for filename in os.listdir(input_dir):
    if filename.lower().endswith(".png"):
        input_path = os.path.join(input_dir, filename)
        output_path = os.path.join(output_dir, filename)

        try:
            with Image.open(input_path) as img:
                # Convert to RGB if not already in RGB mode
                if img.mode != "RGB":
                    print(f"[INFO] Converting {filename} to RGB mode...")
                    img = img.convert("RGB")

                # Resize the image if desired size is specified
                if desired_size:
                    print(f"[INFO] Resizing {filename} to {desired_size}...")
                    img = img.resize(desired_size, Image.Resampling.LANCZOS)

                # Save the image as a standard PNG
                img.save(output_path, format="PNG")
                print(f"[SUCCESS] Processed {filename} -> {output_path}")
        except Exception as e:
            print(f"[ERROR] Failed to process {filename}: {e}")

print("[INFO] Image processing complete.")
