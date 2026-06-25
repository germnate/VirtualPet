import { useEffect, useState, useRef } from "react";
import "./App.css";

const API = "http://192.168.86.29:5058";

export default function App() {
  const [pet, setPet] = useState(null);
  const [currentState, setCurrentState] = useState("idle");
  const [isAnimating, setIsAnimating] = useState(false);

  const animationTimeout = useRef(null);

  // --- API CALLS -------------------------------------------------------------

  async function getPet() {
    const res = await fetch(`${API}/pet`);
    return await res.json();
  }

  async function feedPet() {
    const res = await fetch(`${API}/pet/feed`, { method: "POST" });
    return await res.json();
  }

  async function playPet() {
    const res = await fetch(`${API}/pet/play`, { method: "POST" });
    return await res.json();
  }

  async function sleepPet() {
    const res = await fetch(`${API}/pet/sleep`, { method: "POST" });
    return await res.json();
  }

  async function wakePet() {
    const res = await fetch(`${API}/pet/wake`, { method: "POST" });
    return await res.json();
  }

  // --- REFRESH LOOP ----------------------------------------------------------

  async function refreshPet() {
    if (isAnimating) return; // don't interrupt animations

    const p = await getPet();
    setPet(p);
    setCurrentState(p.state); // idle/happy/sad/etc.
  }

  // --- ACTION HANDLERS -------------------------------------------------------

  async function handleFeed() {
    setIsAnimating(true);
    setCurrentState("eat");

    await feedPet();

    clearTimeout(animationTimeout.current);
    animationTimeout.current = setTimeout(async () => {
      setIsAnimating(false);
      await refreshPet();
      handleWake(); // matches your old JS
    }, 1000);
  }

  async function handlePlay() {
    setIsAnimating(true);
    setCurrentState("play");

    await playPet();

    clearTimeout(animationTimeout.current);
    animationTimeout.current = setTimeout(async () => {
      setIsAnimating(false);
      await refreshPet();
    }, 1000);
  }

  async function handleSleep() {
    setIsAnimating(true);
    setCurrentState("sleep");

    await sleepPet();

    clearTimeout(animationTimeout.current);
    animationTimeout.current = setTimeout(async () => {
      setIsAnimating(false);
      await refreshPet();
    }, 1000);
  }

  async function handleWake() {
    setIsAnimating(true);
    setCurrentState("wake");

    await wakePet();

    clearTimeout(animationTimeout.current);
    animationTimeout.current = setTimeout(async () => {
      setIsAnimating(false);
      await refreshPet();
    }, 1000);
  }

  // --- INITIAL LOAD + INTERVAL ----------------------------------------------

  useEffect(() => {
    // Run refreshPet AFTER the effect completes
    Promise.resolve().then(refreshPet);

    const interval = setInterval(refreshPet, 2000);
    return () => clearInterval(interval);
  }, []);


  // --- RENDER ----------------------------------------------------------------

  if (!pet) return <div>Loading...</div>;

  return (
    <div className="container">
      <img
        id="fox"
        src={`sprites/${currentState}-1.png`}
        alt="fox"
        style={{ width: 200 }}
      />

      <div className="stats">
        <p>Hunger: {pet.hunger}</p>
        <p>Energy: {pet.energy}</p>
        <p>Happiness: {pet.happiness}</p>
        <p>Mood: {pet.mood}</p>
      </div>

      <div className="buttons">
        <button onClick={handleFeed}>Feed</button>
        <button onClick={handlePlay}>Play</button>
        <button onClick={handleSleep}>Sleep</button>
        <button onClick={handleWake}>Wake</button>
      </div>
    </div>
  );
}
