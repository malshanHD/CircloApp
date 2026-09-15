import { useEffect, useRef, useState } from "react";
import { motion, useReducedMotion } from "framer-motion";
import { FiZap, FiSend, FiArrowUpRight } from "react-icons/fi";
import { useAsk } from "../../features/ai/useAsk";
import { ApiError } from "../common/UI";
const suggestions = [
  "How much did we spend on transport?",
  "Summarize this event’s expenses.",
  "Who paid the most?",
  "Show the food-related expenses.",
];
export default function EventAssistant({ eventId, eventName }) {
  const [question, setQuestion] = useState("");
  const [messages, setMessages] = useState([]);
  const [failed, setFailed] = useState(null);
  const [error, setError] = useState(null);
  const end = useRef(null);
  const input = useRef(null);
  const controller = useRef(null);
  const locked = useRef(false);
  const reduced = useReducedMotion();
  const mutation = useAsk(eventId);
  useEffect(
    () => () => {
      controller.current?.abort();
    },
    [],
  );
  useEffect(() => {
    end.current?.scrollIntoView({
      behavior: reduced ? "auto" : "smooth",
      block: "nearest",
    });
  }, [messages, mutation.isPending, error, reduced]);
  async function send(value, retryId) {
    const text = value.trim();
    if (!text || locked.current) return;
    locked.current = true;
    controller.current = new AbortController();
    setError(null);
    setFailed(null);
    const id = retryId || crypto.randomUUID();
    if (!retryId) setMessages((old) => [...old, { id, role: "user", text }]);
    setQuestion("");
    try {
      const result = await mutation.mutateAsync({
        question: text,
        signal: controller.current.signal,
      });
      if (!controller.current.signal.aborted)
        setMessages((old) => [
          ...old,
          { id: id + "-answer", role: "assistant", text: result.answer },
        ]);
    } catch (failure) {
      if (!controller.current.signal.aborted) {
        setError(failure);
        setFailed({ id, question: text });
      }
    } finally {
      locked.current = false;
      input.current?.focus();
    }
  }
  return (
    <section
      className="chat-card card"
      aria-label={`Circlo AI for ${eventName}`}
    >
      <header className="chat-header">
        <span className="ai-icon">
          <FiZap />
        </span>
        <div>
          <h2>Circlo AI</h2>
          <p>One conversation. All about {eventName}.</p>
        </div>
        <span className="badge">EVENT ASSISTANT</span>
      </header>
      <div
        className="chat-messages"
        role="log"
        aria-live="polite"
        aria-relevant="additions"
      >
        {!messages.length && (
          <div className="chat-welcome">
            <span className="chat-spark">✳</span>
            <h3>Let's make sense of it.</h3>
            <p>
              Ask about this event's expenses.
              <br />A little clarity, in your own words.
            </p>
            <div className="suggestions">
              {suggestions.map((text) => (
                <button
                  key={text}
                  onClick={() => send(text)}
                  disabled={mutation.isPending}
                >
                  {text}
                  <FiArrowUpRight />
                </button>
              ))}
            </div>
          </div>
        )}
        {messages.map((message) => (
          <motion.div
            key={message.id}
            initial={{ opacity: 0, y: reduced ? 0 : 7 }}
            animate={{ opacity: 1, y: 0 }}
            className={`chat-message ${message.role}`}
          >
            <span className="message-label">
              {message.role === "assistant"
                ? "Circlo AI · AI-generated"
                : "You"}
            </span>
            <p>{message.text}</p>
          </motion.div>
        ))}
        {mutation.isPending && (
          <div className="thinking" role="status">
            <span />
            <span />
            <span />
            <p>Circlo is thinking…</p>
          </div>
        )}
        {error && (
          <ApiError
            error={error}
            retry={() => send(failed.question, failed.id)}
          />
        )}
        <div ref={end} />
      </div>
      <form
        className="chat-composer"
        onSubmit={(e) => {
          e.preventDefault();
          send(question);
        }}
      >
        <label className="sr-only" htmlFor="ai-question">
          Ask about this event
        </label>
        <textarea
          id="ai-question"
          ref={input}
          rows={2}
          placeholder="Ask anything about this event's expenses…"
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          onKeyDown={(e) => {
            if (
              e.key === "Enter" &&
              !e.shiftKey &&
              !e.nativeEvent.isComposing
            ) {
              e.preventDefault();
              send(question);
            }
          }}
        />
        <button
          className="button primary"
          type="submit"
          aria-label="Send question"
          disabled={!question.trim() || mutation.isPending}
        >
          <FiSend />
        </button>
      </form>
      <p className="chat-disclaimer">
        AI-generated responses can be inaccurate. Verify important financial
        information.<span>Enter to send · Shift + Enter for a new line</span>
      </p>
    </section>
  );
}
