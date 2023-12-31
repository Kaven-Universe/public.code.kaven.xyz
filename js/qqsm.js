/********************************************************************
 * @author:      Kaven
 * @email:       kaven@wuwenkai.com
 * @website:     http://blog.kaven.xyz
 * @file:        [kaven-public] /js/qqsm.js
 * @create:      2023-01-22 16:44:38.451
 * @modify:      2023-01-22 18:39:42.020
 * @times:       6
 * @lines:       48
 * @copyright:   Copyright © 2023 Kaven. All Rights Reserved.
 * @description: [description]
 * @license:     [license]
 ********************************************************************/

if (!window.QQSM) {
    window.QQSM = class {
        static id;

        static click(selector) {
            const el = document.querySelector(selector);
            if (el) {
                el.click();
                return true;
            }

            return false;
        }

        static start(timeout = 1000, index = 1) {
            if (this.id) {
                return;
            }

            let t = 0;
            this.id = setInterval(() => {
                layer.closeAll();
                this.click(`#hb > dl:nth-child(${index}) > dt`);
                console.info(`index: ${index}, try: ${++t}`);
            }, timeout);
        }

        static stop() {
            clearInterval(this.id);
            this.id = undefined;
        }
    }
}
