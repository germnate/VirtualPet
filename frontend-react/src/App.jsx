import { useCallback, useEffect, useRef, useState } from "react";
import VerticalMeter from "./VerticalMeter";
import RadialMeter from "./RadialMeter";
import { feedPet, getPet, playPet, sleepPet, wakePet } from "./api";
import "./App.css";

function getCurrentState(petData) {
  switch (petData.state) {
    case "sleep":
      return "sleep";
    case "play":
      return "play";
    case "eat":
      return "eat";
    default:
      return ["exhausted", "tired", "sad", "miserable", "hungry"].includes(petData.mood)
        ? "sad"
        : "idle";
  }
}

export default function App() {
  const [pet, setPet] = useState(null);
  const [currentState, setCurrentState] = useState("idle");
  const [isAnimating, setIsAnimating] = useState(false);
  const [actionError, setActionError] = useState("");

  const animationTimeout = useRef(null);

  // --- REFRESH LOOP ----------------------------------------------------------

  const refreshPet = useCallback(async () => {
    if (isAnimating) return; // don't interrupt animations

    const p = await getPet();
    setActionError("");
    setPet(p);
    setCurrentState(getCurrentState(p));
  }, [isAnimating]);

  // --- ACTION HANDLERS -------------------------------------------------------

  async function handleFeed() {
    setActionError("");
    setIsAnimating(true);
    setCurrentState("eat");

    try {
      await feedPet();
    } catch (error) {
      setIsAnimating(false);
      setActionError(error.message ?? "Unable to feed right now.");

      if (error.pet) {
        setPet(error.pet);
        setCurrentState(getCurrentState(error.pet));
      } else {
        await refreshPet();
      }

      return;
    }

    clearTimeout(animationTimeout.current);
    animationTimeout.current = setTimeout(async () => {
      setIsAnimating(false);
      await refreshPet();
      handleWake(); // matches your old JS
    }, 1000);
  }

  async function handlePlay() {
    setActionError("");
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
    setActionError("");
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
    setActionError("");
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
  }, [refreshPet]);


  // --- RENDER ----------------------------------------------------------------

  if (!pet) return <div>Loading...</div>;

  return (
    <div className="container">
      <div className="pet-window">
        <VerticalMeter label="Hunger" value={100 - pet.hunger} color="#0a9c27" />
        <VerticalMeter label="Energy" value={pet.energy} color="#4da6ff" />
        <div className="pet-portrait">
          <p className="feeding-count">Feedings available: {pet.remainingFeedings}/3</p>
          <img
            id="fox"
            src={`/sprites/${currentState}-1.png`}
            alt="fox"
            style={{ width: 200 }}
          />
        </div>
        <RadialMeter value={pet.happiness} />
      </div>
      <p>Mood: {pet.mood}</p>
      {actionError ? <p className="action-error">{actionError}</p> : null}
      <div className="buttons">
        <button onClick={handleFeed} disabled={pet.remainingFeedings === 0}>Feed</button>
        <button onClick={handlePlay}>Play</button>
        <button onClick={handleSleep}>Sleep</button>
        <button onClick={handleWake}>Wake</button>
      </div>
    </div>
  );
}
