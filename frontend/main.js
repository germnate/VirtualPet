let frame = 0;
let currentState = "idle";

async function getPet() {
    const res = await fetch("http://192.168.86.29:5058/pet");
    return await res.json();
}

async function feedPet() {
    const res = await fetch("http://192.168.86.29:5058/pet/feed", { method: "POST" });
    return await res.json();
}

async function playPet() {
    const res = await fetch("http://192.168.86.29:5058/pet/play", { method: "POST" });
    return await res.json();
}

async function sleepPet() {
    const res = await fetch("http://192.168.86.29:5058/pet/sleep", { method: "POST" });
    return await res.json();
}

async function wakePet() {
    const res = await fetch("http://192.168.86.29:5058/pet/wake", { method: "POST" });
    return await res.json();
}

async function refreshPet() {
    const pet = await getPet();

    document.getElementById("hunger").textContent = pet.hunger;
    document.getElementById("energy").textContent = pet.energy;
    document.getElementById("happiness").textContent = pet.happiness;
    document.getElementById("mood").textContent = pet.mood;

    currentState = pet.state;
}

async function handleFeed() {
    const pet = await feedPet();
    currentState = pet.state;
    refreshPet();
}

async function handlePlay() {
    const pet = await playPet();
    currentState = pet.state;
    refreshPet();
}

async function handleSleep() {
    const pet = await sleepPet();
    currentState = pet.state;
    refreshPet();
}

async function handleWake() {
    const pet = await wakePet();
    currentState = pet.state;
    refreshPet();
}

function animate() {
    const img = document.getElementById("fox");
    img.src = `sprites/${currentState}-${1}.png`;

    frame = (frame + 1) % 4;
    setTimeout(animate, 400);
}

animate();
refreshPet();
