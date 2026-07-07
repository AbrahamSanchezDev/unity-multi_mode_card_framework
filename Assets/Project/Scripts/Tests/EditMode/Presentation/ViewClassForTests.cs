using NUnit.Framework;
using UnityEngine.UIElements;
using System.Reflection;

namespace CardFramework.Tests.EditMode.Presentation {
    public class ViewClassForTests {
        protected void SimulateButtonClick(Button button) {
            if (button == null || button.clickable == null) return;

            // Obtenemos el método interno 'Invoke' de la clase Clickable a través de reflexión
            var invokeMethod = typeof(Clickable).GetMethod("Invoke",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

            if (invokeMethod != null) {
                using (var clickEvent = ClickEvent.GetPooled()) {
                    clickEvent.target = button;
                    // Invocamos el método directamente sobre el objeto 'clickable' del botón
                    invokeMethod.Invoke(button.clickable, new object[] { clickEvent });
                }
            }
            else {
                Assert.Fail("No se pudo resolver el método interno 'Invoke' en Clickable para simular el click.");
            }
        }

        protected void SetPrivateField(object target, string fieldName, object value) {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null) {
                Assert.Fail($"Field '{fieldName}' no se pudo resolver en {target.GetType().Name}.");
            }
            field.SetValue(target, value);
        }

        protected object GetPrivateField(object target, string fieldName) {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null) {
                Assert.Fail($"Field '{fieldName}' no se pudo resolver en {target.GetType().Name}.");
            }
            return field.GetValue(target);
        }

    }
}