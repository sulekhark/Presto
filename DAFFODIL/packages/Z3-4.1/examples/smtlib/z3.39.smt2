(set-option :auto-config true)
(set-option :produce-models true)

(set-option :auto-config false) ; disable automatic self configuration
(set-option :mbqi false) ; disable model-based quantifier instantiation
(declare-fun f (Int) Int)
(declare-fun g (Int) Int)
(declare-const a Int)
(declare-const b Int)
(declare-const c Int)
(assert (forall ((x Int))
                (! (= (f (g x)) x)
                   :pattern ((f (g x))))))
(assert (= a (g b)))
(assert (= b c))
(assert (not (= (f a) c)))
(check-sat)
