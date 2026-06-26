import { useEffect, useState, useRef } from "react";
import VerticalMeter from "./VerticalMeter";
import RadialMeter from "./RadialMeter";
import { feedPet, getPet, playPet, sleepPet, wakePet } from "./api";
import "./App.css";

export default function App() {
  const [pet, setPet] = useState(null);
  const [currentState, setCurrentState] = useState("idle");
  const [isAnimating, setIsAnimating] = useState(false);

  const animationTimeout = useRef(null);

  // --- REFRESH LOOP ----------------------------------------------------------

  function calcuateAndSetCurrentState(petData) {
    // Determine the current state based on petData
    console.log('here', petData)
    switch (petData.state) {
      case "sleep":
        setCurrentState("sleep");
        break;
      case "play":
        setCurrentState("play");
        break;
      case "eat":
        setCurrentState("eat");
        break;
      default:
        if (["exhausted", "tired", "sad", "miserable", "hungry"].includes(petData.mood)) {
          setCurrentState("sad");
          console.log('sad');
        } else {
          setCurrentState("idle");
        }
    }
  }

  async function refreshPet() {
    if (isAnimating) return; // don't interrupt animations

    const p = await getPet();
    setPet(p);
    calcuateAndSetCurrentState(p); // update currentState based on pet state
    // setCurrentState(p.state); // idle/happy/sad/etc.
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
      <div className="pet-window">
        <VerticalMeter label="Hunger" value={100 - pet.hunger} color="#0a9c27" />
        <VerticalMeter label="Energy" value={pet.energy} color="#4da6ff" />
        <img
          id="fox"
          src={`/sprites/${currentState}-1.png`}
          alt="fox"
          style={{ width: 200 }}
        />
        <RadialMeter value={pet.happiness} />
      </div>
      <p>Mood: {pet.mood}</p>
      <div className="buttons">
        <button onClick={handleFeed}>Feed</button>
        <button onClick={handlePlay}>Play</button>
        <button onClick={handleSleep}>Sleep</button>
        <button onClick={handleWake}>Wake</button>
      </div>
    </div>
  );
}
