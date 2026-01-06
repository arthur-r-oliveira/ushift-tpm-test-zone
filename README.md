# ushift-tpm-test-zone

A proof-of-concept (PoC) demonstrating secure, non-privileged access to a Hardware Root of Trust (TPM 2.0) within **Red Hat build of MicroShift** using the **Generic Device Plugin (GDP)**.

## Overview

This project addresses a critical security requirement: allowing containerized applications (specifically .NET/C#) to interact with a TPM for cryptographic operations without requiring the container to run as `root` or with `privileged: true`.

### Key Achievements

* **Zero Elevation**: The application runs with `runAsNonRoot: true` and `allowPrivilegeEscalation: false`.
* **Strict Security**: Complies with the Kubernetes `restricted` Pod Security Admission (PSA) profile.
* **Hardware Passthrough**: Leverages the MicroShift Generic Device Plugin (GDP) to map `/dev/tpmrm0` into the pod.

---

## Architecture & Hardware Access Mechanism

The solution bridges the gap between the host hardware and an unprivileged container through several layers of abstraction:

### How the TPM is Accessed

Based on the implementation in `Program.cs`, the application does not interact with the character device directly via C# file streams. Instead, it uses **Process Delegation**:

1. **Process Invocation**: The .NET runtime spawns the `tpm2_getrandom` binary from the standard `tpm2-tools` suite.
2. **TCTI Layer**: The `tpm2-tools` utilize the **TPM Command Transmission Interface (TCTI)** to locate the in-kernel resource manager at `/dev/tpmrm0`.
3. **GDP Mapping**: The **Generic Device Plugin (GDP)** ensures that `/dev/tpmrm0` is available at the correct path inside the pod's filesystem with the necessary cgroup permissions, despite the pod being unprivileged.
4. **Output Capture**: The .NET application redirects the standard output of the tool to consume the cryptographic results.

---

## Prerequisites

* **Red Hat build of MicroShift 4.20+**.
* **Generic Device Plugin Enabled**: Configured in `/etc/microshift/config.yaml`.

~~~
genericDevicePlugin:
  status: Enabled
  domain: device.microshift.io
  devices:
   - name: host-tpm
     groups:
      - paths:
         - path: /dev/tpmrm0
~~~

---

## Repository Structure

* `Program.cs`: .NET 8 source code using `ProcessStartInfo` to delegate commands to TPM tools.
* `Containerfile`: Builds a UBI 9 image containing the .NET SDK 8.0 and `tpm2-tools`.
* `k8s/`:
* `namespace.yaml`: Defines the `tpm-test-zone` with restricted PSA labels.
* `pod-tpm.yaml`: The pod manifest requesting the `host-tpm` resource and dropping all capabilities.
* `kustomization.yaml`: Orchestrates the deployment using the modern `labels` syntax.



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

```bash
oc logs -n tpm-test-zone tpm-test-app

```

**Successful Output Example:**

```text
--- TPM Access Test Application ---
[SUCCESS] Found TPM Resource Manager at /dev/tpmrm0
Requesting 16 random bytes from hardware TPM...
[SUCCESS] TPM Data: e8d51eb798087253e5098e43653c4707

```

---

## Support Status

The **Generic Device Plugin** is a **Technology Preview** feature in Red Hat build of MicroShift. While it successfully enables unprivileged TPM access, it is not yet intended for production Service Level Agreements (SLAs).

