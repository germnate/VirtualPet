const API = "http://192.168.86.29:5058";

async function request(path, options) {
  const response = await fetch(`${API}${path}`, options);
  const data = await response.json();

  if (!response.ok) {
    const error = new Error(data?.message ?? "Request failed.");
    error.status = response.status;
    error.pet = data?.pet ?? null;
    throw error;
  }

  return data;
}

export function getPet() {
  return request("/pet");
}

export function feedPet() {
  return request("/pet/feed", { method: "POST" });
}

export function playPet() {
  return request("/pet/play", { method: "POST" });
}

export function sleepPet() {
  return request("/pet/sleep", { method: "POST" });
}

export function wakePet() {
  return request("/pet/wake", { method: "POST" });
}