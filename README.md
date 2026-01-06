# ushift-tpm-test-zone

A proof-of-concept (PoC) demonstrating secure, non-privileged access to a Hardware Root of Trust (TPM 2.0) within **Red Hat build of MicroShift** using the **Generic Device Plugin (GDP)**.

## Overview

This project addresses a critical security requirement: allowing containerized applications (specifically .NET/C#) to interact with a TPM for cryptographic operations (e.g., generating random numbers, CSRs, or TLS keys) without requiring the container to run as `root` or with `privileged: true`.

### Key Achievements

* **Zero Elevation**: The application runs with `runAsNonRoot: true` and `allowPrivilegeEscalation: false`.
* **Strict Security**: Complies with the Kubernetes `restricted` Pod Security Admission (PSA) profile.
* **Hardware Passthrough**: Leverages the MicroShift Generic Device Plugin (GDP) to map `/dev/tpmrm0` into the pod.

---

## Architecture

The solution relies on three distinct layers of security and orchestration:

1. **MicroShift GDP**: Advertises the host TPM as a schedulable resource (`device.microshift.io/host-tpm`).
2. **Namespace Policy**: Enforces the `restricted` security profile via standard labels.
3. **App Runtime**: A .NET 8.0 application utilizing `tpm2-tools` to interact with the TPM resource manager.

---

## Prerequisites

* **Red Hat build of MicroShift 4.20+**.
* **Generic Device Plugin Enabled**: Configured in `/etc/microshift/config.yaml`.
* **Host-level Permissions**: A `udev` rule on the host granting access to the TPM device for the container's group/user ID (e.g., `1001`).

---

## Repository Structure

* `Program.cs`: .NET 8 source code that attempts to read random bytes from the TPM.
* `Containerfile`: Builds a UBI 9-based image containing the .NET runtime and `tpm2-tools`.
* `k8s/`:
* `namespace.yaml`: Defines the `tpm-test-zone` with restricted PSA labels.
* `pod-tpm.yaml`: The pod manifest requesting the `host-tpm` resource.
* `kustomization.yaml`: Orchestrates the deployment.



---

## Usage

### 1. Build the Image

```bash
podman build -t tpm-test-app .

```

### 2. Deploy to MicroShift

```bash
oc apply -k k8s/

```

### 3. Verify Results

Check the logs to confirm the TPM was accessed successfully:

```bash
oc logs -n tpm-test-zone tpm-test-app

```

**Expected Output:**

```text
--- TPM Access Test Application ---
[SUCCESS] Found TPM Resource Manager at /dev/tpmrm0
Requesting 16 random bytes from hardware TPM...
[SUCCESS] TPM Data: <hex-string>

```

---

## Support Status

This implementation uses the **Generic Device Plugin**, which is currently a **Technology Preview** feature in Red Hat build of MicroShift 4.20. At the time of this writting, it is intended for evaluation and development purposes and is not yet recommended for production workloads. 
