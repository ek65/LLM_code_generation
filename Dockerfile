# Use an official Python runtime based on Debian/Ubuntu
FROM python:3.11-slim

# Install dependencies: Tk interface, SSL libraries, certificates, git, OpenGL library, and GLib library (for libgthread)
RUN apt-get update && \
    apt-get install -y python3-tk libssl-dev ca-certificates git libgl1-mesa-glx libglib2.0-0 && \
    rm -rf /var/lib/apt/lists/*

# Create a non-root user for Cloud Run
RUN adduser --disabled-password --gecos '' appuser

# Accept build-time argument for the GitHub token
ARG GITHUB_TOKEN

# Clone the private repository from the main branch using your GitHub username
RUN git clone --branch main https://ek65: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@github.com/ek65/AuthorExercise.git /AuthorExercise

# Set working directory to the Scenic-main subfolder of the cloned repository
WORKDIR /AuthorExercise/Scenic-main

# (Optional) If you want to automatically update the target_script variable in scenic_avatar.py,
# uncomment and modify the line below. This example sets it to a file in the scenic_output directory.
# RUN sed -i 's|^target_script = .*|target_script = "program_synthesis/scenic_output/your_script.scenic"|' Scenic/src/scenic/simulators/unity/verifai/scenic_avatar.py

# Create a virtual environment called venv in the repository
RUN python3 -m venv venv

# Update PATH so that the virtual environment’s binaries are used by default
ENV PATH="/AuthorExercise/Scenic-main/venv/bin:$PATH"

# Upgrade pip in the virtual environment
RUN pip install --upgrade pip

# (Optional) Install repository dependencies if a requirements.txt exists
RUN if [ -f requirements.txt ]; then pip install -r requirements.txt; fi

# Install local packages from the subfolders Scenic and VerifAI in editable mode
RUN pip install -e ./Scenic
RUN pip install -e ./VerifAI
RUN pip install openai
RUN pip install requests
RUN pip install pyzmq

# Change ownership of the repository directory to the non-root user
RUN chown -R appuser:appuser /AuthorExercise

# Switch to the non-root user
USER appuser

# Expose the port that Cloud Run expects (default 8080)
EXPOSE 5555
ENV PORT=5555

# Define the default command to run the scenic_avatar.py script
CMD ["python", "Scenic/src/scenic/simulators/unity/verifai/scenic_avatar.py"]
