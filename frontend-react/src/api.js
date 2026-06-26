const API = "http://192.168.86.29:5058";

async function request(path, options) {
  const response = await fetch(`${API}${path}`, options);
  return await response.json();
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